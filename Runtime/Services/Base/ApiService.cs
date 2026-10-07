using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Core;
using Deucarian.API.Calls;
using Deucarian.API.Models;
using Newtonsoft.Json.Linq;

namespace Deucarian.API.Services.Base
{
    /// <summary>
    /// Don't have to create a new apiservice for each call, can just call ApiServices class which will instantiate these services when required
    /// </summary>
    /// <typeparam name="TResponse">Represents type we expect in response from Backend</typeparam>
    /// <remarks>
    /// TReponse type does not have to match the Body type, the Body we send has nothing to do with this type!
    /// </remarks>
    internal class ApiService<TResponse>
    {
        protected HttpMethod HttpMethod { get; }

        public ApiService(HttpMethod httpMethod)
        {
            HttpMethod = httpMethod;
        }

        public virtual async Task<ApiCallResult<TResponse>> ExecuteAsync(
                string endpoint,
                bool requiresAuthentication,
                object data = null,
                string accessToken = null,
                Dictionary<string, string> customHeaders = null,
                [CallerMemberName] string callerMemberName = "",
                [CallerFilePath] string callerFilePath = "",
                [CallerLineNumber] int callerLineNumber = 0,
                CancellationToken cancellationToken = default)
        {
            try
            {
                ApiCall<TResponse> apiCall = ApiCall<TResponse>.GetApiCall<TResponse>(
                                                                                      endpoint,
                                                                                      this.HttpMethod,
                                                                                      requiresAuthentication,
                                                                                      data,
                                                                                      accessToken);

                ApiCallResult<TResponse> result = await apiCall.Execute(customHeaders, cancellationToken);


                if (!result.IsSuccess)
                {
                    HandleApiFailure(result, endpoint, callerMemberName, callerFilePath, callerLineNumber);
                }

                string extractedMessage = result.IsSuccess
                                                  ? null
                                                  : TryExtractJsonMessage(result.ErrorMessage) ?? result.ErrorMessage;

                result.ErrorMessage = extractedMessage;
                return result;
            }
            catch (Exception ex)
            {
                ApiLog.Requests.Error("The legacy API request could not be completed. Inspect its ApiError for details.");
                return new ApiCallResult<TResponse>
                {
                        IsSuccess = false,
                        Error = new ApiError
                        {
                            Message = ex is OperationCanceledException
                                ? "API request was canceled."
                                : "The API request could not be completed.",
                            Exception = ex,
                            IsCancellation = ex is OperationCanceledException
                        },
                        HttpMethod = this.HttpMethod
                };
            }
        }

        public void Execute(string endpoint,
                            bool requiresAuthentication,
                            Action<TResponse> onSuccess,
                            Action<ApiError> onFailure,
                            object data = null,
                            string accessToken = null,
                            Dictionary<string, string> customHeaders = null,
                            CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run<TResponse>(async () => await ExecuteAsync(endpoint,
                                                                      requiresAuthentication,
                                                                      data,
                                                                      accessToken,
                                                                      customHeaders,
                                                                      cancellationToken: cancellationToken
                                       ),
                                       onSuccess,
                                       onFailure,
                                       cancellationToken
            );
        }

        public string GetIndentedJson(string rawJson)
        {
            string output = rawJson;
            try
            {
                JToken parsed = JToken.Parse(rawJson);

                output = parsed.ToString(Newtonsoft.Json.Formatting.Indented);
            }
            catch
            {}

            return output;
        }

        protected void HandleApiFailure(ApiCallResult<TResponse> result, string endpoint, string callerMemberName, string callerFilePath, int callerLineNumber)
        {
            ApiLog.Requests.Error("A legacy API request failed. Inspect its ApiError for details.");
        }

        /// <summary>
        /// Tries to extract the message from the JSON object if the errorMessage contains one.
        /// It looks for "Response:" in the string and attempts to parse the JSON that follows.
        /// </summary>
        /// <param name="errorMessage">The full error message string.</param>
        /// <returns>The extracted message if available; otherwise, null.</returns>
        private string TryExtractJsonMessage(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                return null;

            try
            {
                int responseIndex = errorMessage.IndexOf("Response:", StringComparison.OrdinalIgnoreCase);
                if (responseIndex >= 0)
                {
                    int jsonStart = errorMessage.IndexOf('{', responseIndex);
                    if (jsonStart >= 0)
                    {
                        string jsonSubstring = errorMessage.Substring(jsonStart);
                        var json = JObject.Parse(jsonSubstring);
                        if (json.TryGetValue("message", StringComparison.OrdinalIgnoreCase, out JToken messageToken))
                        {
                            return messageToken.ToString();
                        }
                    }
                }
                else
                {
                    int firstBrace = errorMessage.IndexOf('{');
                    if (firstBrace >= 0)
                    {
                        string jsonSubstring = errorMessage.Substring(firstBrace);
                        var json = JObject.Parse(jsonSubstring);
                        if (json.TryGetValue("message", StringComparison.OrdinalIgnoreCase, out JToken messageToken))
                        {
                            return messageToken.ToString();
                        }
                    }
                }
            }
            catch (Exception)
            {
                ApiLog.Requests.Warning("A legacy API failure message could not be parsed.");
            }
            return null;
        }
    }
}
