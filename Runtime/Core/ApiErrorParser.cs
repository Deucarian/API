using System;
using System.Collections.Generic;
using JorisHoef.APIHelper.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JorisHoef.APIHelper.Core
{
    internal sealed class ApiErrorParser : IApiErrorParser
    {
        public ApiError Parse(ApiRequest request, ApiTransportResponse response, Exception exception = null)
        {
            ApiError error = new ApiError
            {
                    HttpStatusCode = response?.StatusCode,
                    RequestUrl = response?.RequestUrl ?? request?.Endpoint,
                    RawResponseBody = response?.RawBody,
                    ResponseHeaders = CopyHeaders(response?.ResponseHeaders),
                    TransportError = response?.TransportError,
                    Exception = exception
            };

            ApplyBackendError(response?.RawBody, error);

            if (string.IsNullOrWhiteSpace(error.Message))
            {
                error.Message = exception?.Message
                                ?? error.BackendMessage
                                ?? response?.TransportError
                                ?? BuildHttpStatusMessage(response);
            }

            error.IsTimeout = LooksLikeTimeout(error.TransportError) || LooksLikeTimeout(error.Message);
            return error;
        }

        private static void ApplyBackendError(string rawBody, ApiError error)
        {
            if (string.IsNullOrWhiteSpace(rawBody) || error == null)
            {
                return;
            }

            try
            {
                JToken token = JToken.Parse(rawBody);
                if (token.Type != JTokenType.Object)
                {
                    error.BackendMessage = token.Type == JTokenType.String ? token.ToString() : null;
                    error.Message = error.BackendMessage;
                    return;
                }

                JObject obj = (JObject)token;
                error.BackendCode = ReadFirstString(obj, "code", "errorCode", "error_code", "type");

                string title = ReadFirstString(obj, "title");
                string detail = ReadFirstString(obj, "detail");
                string message = ReadFirstString(obj,
                                                 "message",
                                                 "error_description",
                                                 "error");

                if (string.IsNullOrWhiteSpace(message))
                {
                    if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(detail))
                    {
                        message = title + ": " + detail;
                    }
                    else
                    {
                        message = title ?? detail;
                    }
                }

                error.BackendMessage = message;
                error.Message = message;

                if (obj.TryGetValue("errors", StringComparison.OrdinalIgnoreCase, out JToken errorsToken))
                {
                    error.ValidationErrors = ExtractValidationErrors(errorsToken);
                }
            }
            catch (JsonException)
            {
                error.BackendMessage = rawBody;
            }
        }

        private static string ReadFirstString(JObject obj, params string[] names)
        {
            foreach (string name in names)
            {
                if (obj.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out JToken token)
                    && token != null
                    && token.Type != JTokenType.Null)
                {
                    string value = token.Type == JTokenType.String
                                           ? token.Value<string>()
                                           : token.ToString(Formatting.None);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }

            return null;
        }

        private static Dictionary<string, string[]> ExtractValidationErrors(JToken errorsToken)
        {
            Dictionary<string, string[]> validationErrors = new Dictionary<string, string[]>();

            if (errorsToken == null || errorsToken.Type == JTokenType.Null)
            {
                return validationErrors;
            }

            if (errorsToken.Type == JTokenType.Object)
            {
                foreach (JProperty property in ((JObject)errorsToken).Properties())
                {
                    validationErrors[property.Name] = ToMessages(property.Value);
                }

                return validationErrors;
            }

            validationErrors["errors"] = ToMessages(errorsToken);
            return validationErrors;
        }

        private static string[] ToMessages(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return Array.Empty<string>();
            }

            if (token.Type == JTokenType.Array)
            {
                List<string> values = new List<string>();
                foreach (JToken child in token.Children())
                {
                    values.Add(child.Type == JTokenType.String
                                       ? child.Value<string>()
                                       : child.ToString(Formatting.None));
                }

                return values.ToArray();
            }

            return new[]
            {
                    token.Type == JTokenType.String
                            ? token.Value<string>()
                            : token.ToString(Formatting.None)
            };
        }

        private static string BuildHttpStatusMessage(ApiTransportResponse response)
        {
            if (response == null)
            {
                return "API request failed.";
            }

            return "API request failed with HTTP status code " + response.StatusCode + ".";
        }

        private static Dictionary<string, string> CopyHeaders(Dictionary<string, string> headers)
        {
            Dictionary<string, string> copy =
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (headers == null)
            {
                return copy;
            }

            foreach (KeyValuePair<string, string> header in headers)
            {
                if (!string.IsNullOrWhiteSpace(header.Key))
                {
                    copy[header.Key] = header.Value;
                }
            }

            return copy;
        }

        private static bool LooksLikeTimeout(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                   && value.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
