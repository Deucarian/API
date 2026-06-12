using Deucarian.API.Configuration;
using Deucarian.API.Models;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Deucarian.API.Core
{
    internal sealed class UnityApiLogger : IApiLogger
    {
        private readonly ApiClientConfig _config;

        public UnityApiLogger(ApiClientConfig config)
        {
            _config = config;
        }

        public void LogRequest(ApiRequest request, string requestUrl)
        {
            if (_config == null || _config.LoggingMode != ApiLoggingMode.Verbose)
            {
                return;
            }

            Debug.Log("[API] " + request.Method + " " + requestUrl);
        }

        public void LogResponse<TResponse>(ApiResult<TResponse> result)
        {
            if (result == null || _config == null || _config.LoggingMode == ApiLoggingMode.None)
            {
                return;
            }

            if (!result.IsSuccess)
            {
                LogError(result.Error);
                return;
            }

            if (_config.LoggingMode == ApiLoggingMode.Verbose)
            {
                Debug.Log("[API] " + result.HttpMethod + " " + result.RequestUrl + " -> " + result.HttpStatusCode);
            }

            if ((_config.LogRawJson || APIDebugSettings.LogRawJson) && !string.IsNullOrWhiteSpace(result.RawResponseBody))
            {
                Debug.Log("[API] JSON Response from URL: " + result.RequestUrl + "\nJSON:\n" +
                          GetIndentedJson(result.RawResponseBody));
            }
        }

        public void LogError(ApiError error)
        {
            if (error == null || _config == null || _config.LoggingMode == ApiLoggingMode.None)
            {
                return;
            }

            Debug.LogError("[API] Error " + error.HttpStatusCode + " " + error.RequestUrl + ": " + error.Message);
        }

        private static string GetIndentedJson(string rawJson)
        {
            try
            {
                return JToken.Parse(rawJson).ToString(Newtonsoft.Json.Formatting.Indented);
            }
            catch
            {
                return rawJson;
            }
        }
    }
}
