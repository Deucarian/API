using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Calls;
using Deucarian.API.Models;
using Deucarian.API.Services.Base;

namespace Deucarian.API.Services.MultipartForm
{
    /// <summary>
    /// For any multiform data
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    internal class MultipartApiService<TResponse> : ApiService<TResponse>
    {
        public MultipartApiService(HttpMethod httpMethod) : base(httpMethod) { }

        public override async Task<ApiCallResult<TResponse>> ExecuteAsync(string endpoint,
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
                ApiCall<TResponse> apiCall = MultipartFormApiCall<TResponse>.GetApiCall<TResponse>(
                                                                                                   endpoint,
                                                                                                   this.HttpMethod,
                                                                                                   requiresAuthentication,
                                                                                                   data,
                                                                                                   accessToken
                                                                                                  );

                ApiCallResult<TResponse> result = await apiCall.Execute(customHeaders, cancellationToken);
                if (!result.IsSuccess)
                {
                    base.HandleApiFailure(result, endpoint, callerMemberName, callerFilePath, callerLineNumber);
                }
                return result;
            }
            catch(Exception ex)
            {
                ApiLog.Requests.Error("The legacy multipart request could not be completed. Inspect its ApiError for details.");

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
    }
}
