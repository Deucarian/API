using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace Deucarian.API.Core
{
    internal sealed class UnityWebRequestSender : IRequestSender
    {
        public async Task<ApiTransportResponse> SendAsync(UnityWebRequest request,
                                                          ApiRequest apiRequest,
                                                          CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (cancellationToken.Register(request.Abort))
            {
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                ReportProgress(request, apiRequest, false);
                while (!operation.isDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ReportProgress(request, apiRequest, false);
                    await Task.Yield();
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            ReportProgress(request, apiRequest, true);

            bool isAssetBundleResponse = request.downloadHandler is DownloadHandlerAssetBundle;
            byte[] rawBytes = isAssetBundleResponse ? null : request.downloadHandler?.data;
            string textureDecodeError;
            Texture2D texture = TryGetTexture(request, out textureDecodeError);
            string assetBundleDecodeError;
            AssetBundle assetBundle = TryGetAssetBundle(request, out assetBundleDecodeError);

            return new ApiTransportResponse
            {
                    StatusCode = request.responseCode,
                    RequestUrl = request.url,
                    RawBody = GetRawBody(request, rawBytes),
                    RawBytes = rawBytes,
                    Texture = texture,
                    AssetBundle = assetBundle,
                    TransportError = CombineTransportErrors(request.error,
                                                           textureDecodeError,
                                                           assetBundleDecodeError),
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

        private static AssetBundle TryGetAssetBundle(UnityWebRequest request, out string decodeError)
        {
            decodeError = null;
            if (!(request.downloadHandler is DownloadHandlerAssetBundle))
            {
                return null;
            }

            try
            {
                return DownloadHandlerAssetBundle.GetContent(request);
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

        private static string CombineTransportErrors(string transportError,
                                                     string textureDecodeError,
                                                     string assetBundleDecodeError)
        {
            string combined = transportError;
            if (!string.IsNullOrWhiteSpace(textureDecodeError))
            {
                string decodeMessage = "Texture decode failed: " + textureDecodeError;
                combined = string.IsNullOrWhiteSpace(combined)
                                   ? decodeMessage
                                   : combined + " " + decodeMessage;
            }

            if (!string.IsNullOrWhiteSpace(assetBundleDecodeError))
            {
                string decodeMessage = "AssetBundle decode failed: " + assetBundleDecodeError;
                combined = string.IsNullOrWhiteSpace(combined)
                                   ? decodeMessage
                                   : combined + " " + decodeMessage;
            }

            return combined;
        }

        private static void ReportProgress(UnityWebRequest request,
                                           ApiRequest apiRequest,
                                           bool isDone)
        {
            if (request == null || apiRequest?.TransferProgress == null)
            {
                return;
            }

            apiRequest.TransferProgress.Invoke(ApiTransferProgress.Create(
                request.downloadProgress,
                request.uploadProgress,
                ClampBytes(request.downloadedBytes),
                ClampBytes(request.uploadedBytes),
                isDone));
        }

        private static long ClampBytes(ulong bytes)
        {
            return bytes > (ulong)long.MaxValue ? long.MaxValue : (long)bytes;
        }
    }
}
