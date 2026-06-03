using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JorisHoef.APIHelper.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace JorisHoef.APIHelper.Core
{
    internal sealed class UnityWebRequestSender : IRequestSender
    {
        public async Task<ApiTransportResponse> SendAsync(UnityWebRequest request,
                                                          CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (cancellationToken.Register(request.Abort))
            {
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Yield();
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            byte[] rawBytes = request.downloadHandler?.data;
            string textureDecodeError;
            Texture2D texture = TryGetTexture(request, out textureDecodeError);

            return new ApiTransportResponse
            {
                    StatusCode = request.responseCode,
                    RequestUrl = request.url,
                    RawBody = GetRawBody(request, rawBytes),
                    RawBytes = rawBytes,
                    Texture = texture,
                    TransportError = CombineTransportErrors(request.error, textureDecodeError),
                    UnityResult = request.result,
                    ResponseHeaders = request.GetResponseHeaders() ?? new Dictionary<string, string>()
            };
        }

        private static Texture2D TryGetTexture(UnityWebRequest request, out string decodeError)
        {
            decodeError = null;
            if (!(request.downloadHandler is DownloadHandlerTexture))
            {
                return null;
            }

            try
            {
                return DownloadHandlerTexture.GetContent(request);
            }
            catch (Exception ex)
            {
                decodeError = ex.Message;
                return null;
            }
        }

        private static string GetRawBody(UnityWebRequest request, byte[] rawBytes)
        {
            if (request.downloadHandler is DownloadHandlerBuffer)
            {
                return request.downloadHandler.text;
            }

            return IsErrorResponse(request) ? TryDecodeText(rawBytes) : null;
        }

        private static bool IsErrorResponse(UnityWebRequest request)
        {
            return request != null
                   && (request.result != UnityWebRequest.Result.Success || request.responseCode >= 400);
        }

        private static string TryDecodeText(byte[] rawBytes)
        {
            if (rawBytes == null || rawBytes.Length == 0)
            {
                return null;
            }

            string text = Encoding.UTF8.GetString(rawBytes);
            return LooksLikeText(text) ? text : null;
        }

        private static bool LooksLikeText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            int nonPrintableCount = 0;
            foreach (char character in value)
            {
                if (char.IsControl(character)
                    && character != '\r'
                    && character != '\n'
                    && character != '\t')
                {
                    nonPrintableCount++;
                }
            }

            return nonPrintableCount == 0;
        }

        private static string CombineTransportErrors(string transportError, string textureDecodeError)
        {
            if (string.IsNullOrWhiteSpace(textureDecodeError))
            {
                return transportError;
            }

            string decodeMessage = "Texture decode failed: " + textureDecodeError;
            return string.IsNullOrWhiteSpace(transportError)
                           ? decodeMessage
                           : transportError + " " + decodeMessage;
        }
    }
}
