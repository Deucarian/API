using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Deucarian.API.Calls;
using Deucarian.API.Models;
using Deucarian.API.Services.Base;
using UnityEngine;

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
                                                                          [CallerLineNumber] int callerLineNumber = 0)
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

                ApiCallResult<TResponse> result = await apiCall.Execute(customHeaders);
                if (!result.IsSuccess)
                {
                    base.HandleApiFailure(result, endpoint, callerMemberName, callerFilePath, callerLineNumber);
                }
                return new ApiCallResult<TResponse>
                {
                        IsSuccess = result.IsSuccess,
                        Data = result.IsSuccess ? result.Data : default,
                        ErrorMessage = result.ErrorMessage,
                        Exception = result.Exception,
                        HttpMethod = this.HttpMethod,
                        RawResponseBody = result.RawResponseBody
                };
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);

                return new ApiCallResult<TResponse>
                {
                        IsSuccess = false,
                        ErrorMessage = ex.Message,
                        Exception = ex,
                        HttpMethod = this.HttpMethod
                };
            }
        }
    }
}
