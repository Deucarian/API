using System;
using Deucarian.API.Models;
using UnityEngine;

namespace Deucarian.API.Core
{
    internal static class ApiResponseFormatUtility
    {
        public const string JsonAcceptHeader = "application/json";
        public const string TextAcceptHeader = "text/plain,*/*";
        public const string BytesAcceptHeader = "*/*";
        public const string TextureAcceptHeader = "image/png,image/jpeg,image/webp,*/*";

        public const string JsonContentType = "application/json";
        public const string TextContentType = "text/plain";
        public const string BytesContentType = "application/octet-stream";

        public static ApiResponseFormat Resolve<TResponse>(
                ApiResponseFormat requestFormat,
                ApiResponseFormat defaultFormat)
        {
            if (requestFormat != ApiResponseFormat.Auto)
            {
                return requestFormat;
            }

            Type responseType = typeof(TResponse);
            if (responseType == typeof(string))
            {
                return ApiResponseFormat.Text;
            }

            if (responseType == typeof(byte[]))
            {
                return ApiResponseFormat.Bytes;
            }

            if (typeof(Texture2D).IsAssignableFrom(responseType))
            {
                return ApiResponseFormat.Texture;
            }

            return defaultFormat == ApiResponseFormat.Auto
                           ? ApiResponseFormat.Json
                           : defaultFormat;
        }

        public static string GetAcceptHeader(ApiResponseFormat responseFormat)
        {
            switch (responseFormat)
            {
                case ApiResponseFormat.Text:
                    return TextAcceptHeader;

                case ApiResponseFormat.Bytes:
                    return BytesAcceptHeader;

                case ApiResponseFormat.Texture:
                    return TextureAcceptHeader;

                case ApiResponseFormat.Json:
                case ApiResponseFormat.Auto:
                default:
                    return JsonAcceptHeader;
            }
        }

        public static string GetDefaultContentType(ApiRequestBodyFormat bodyFormat, object body)
        {
#pragma warning disable CS0618
            switch (bodyFormat)
            {
                case ApiRequestBodyFormat.RawText:
                    return TextContentType;

                case ApiRequestBodyFormat.RawBytes:
                    return BytesContentType;

                case ApiRequestBodyFormat.Raw:
                    return body is byte[] ? BytesContentType : TextContentType;

                case ApiRequestBodyFormat.Json:
                    return JsonContentType;

                case ApiRequestBodyFormat.MultipartForm:
                default:
                    return null;
            }
#pragma warning restore CS0618
        }
    }
}
