using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Models;
using UnityEngine.Networking;

namespace Deucarian.API.Core
{
    internal sealed class ApiClient : IApiClient
    {
        private readonly IRequestBuilder _requestBuilder;
        private readonly IRequestSender _requestSender;
        private readonly IApiResponseParser _responseParser;
        private readonly IApiErrorParser _errorParser;
        private readonly IApiLogger _logger;
        private readonly ApiResponseFormat _defaultResponseFormat;

        internal ApiClient(IRequestBuilder requestBuilder,
                           IRequestSender requestSender,
                           IApiResponseParser responseParser,
                           IApiErrorParser errorParser,
                           IApiLogger logger,
                           ApiResponseFormat defaultResponseFormat = ApiResponseFormat.Auto)
        {
            _requestBuilder = requestBuilder ?? throw new ArgumentNullException(nameof(requestBuilder));
            _requestSender = requestSender ?? throw new ArgumentNullException(nameof(requestSender));
            _responseParser = responseParser ?? throw new ArgumentNullException(nameof(responseParser));
            _errorParser = errorParser ?? throw new ArgumentNullException(nameof(errorParser));
            _logger = logger;
            _defaultResponseFormat = defaultResponseFormat;
        }

        public async Task<ApiResult<TResponse>> SendAsync<TResponse>(ApiRequest request,
                                                                     CancellationToken cancellationToken = default)
        {
            UnityWebRequest unityRequest = null;
            ApiTransportResponse transportResponse = null;

            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                ApiResponseFormat responseFormat =
                        ApiResponseFormatUtility.Resolve<TResponse>(request.ResponseFormat, _defaultResponseFormat);

                unityRequest = await _requestBuilder.BuildAsync(request, responseFormat, cancellationToken);
                _logger?.LogRequest(request, unityRequest.url);

                transportResponse = await _requestSender.SendAsync(unityRequest, cancellationToken);
                ApiResult<TResponse> result;
                if (transportResponse.IsSuccessStatusCode
                    && transportResponse.UnityResult == UnityWebRequest.Result.Success)
                {
                    result = _responseParser.Parse<TResponse>(request, transportResponse, responseFormat);
                }
                else
                {
                    ApiError error = _errorParser.Parse(request, transportResponse);
                    result = ApiResult<TResponse>.Failure(error, request.Method);
                }

                _logger?.LogResponse(result);
                return result;
            }
            catch (OperationCanceledException ex)
            {
                ApiError error = new ApiError
                {
                        Message = "API request was canceled.",
                        Exception = ex,
                        IsCancellation = true,
                        RequestUrl = unityRequest?.url ?? request?.Endpoint,
                        RawResponseBody = transportResponse?.RawBody,
                        ResponseHeaders = CopyHeaders(transportResponse?.ResponseHeaders),
                        HttpStatusCode = transportResponse?.StatusCode
                };
                ApiResult<TResponse> result =
                        ApiResult<TResponse>.Failure(error, request?.Method ?? HttpMethod.GET);
                _logger?.LogResponse(result);
                return result;
            }
            catch (Exception ex)
            {
                ApiError error = _errorParser.Parse(request, transportResponse, ex);
                ApiResult<TResponse> result =
                        ApiResult<TResponse>.Failure(error, request?.Method ?? HttpMethod.GET);
                _logger?.LogResponse(result);
                return result;
            }
            finally
            {
                unityRequest?.Dispose();
            }
        }

        public Task<ApiResult<TResponse>> SendAsync<TResponse>(ApiEndpoint endpoint,
                                                               CancellationToken cancellationToken = default)
        {
            return SendAsync<TResponse>(endpoint, null, cancellationToken);
        }

        public Task<ApiResult<TResponse>> SendAsync<TResponse>(ApiEndpoint endpoint,
                                                               object body,
                                                               CancellationToken cancellationToken = default)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            return SendAsync<TResponse>(endpoint.CreateRequest(body), cancellationToken);
        }

        public Task<ApiResult<TResponse>> GetAsync<TResponse>(string endpoint,
                                                              CancellationToken cancellationToken = default)
        {
            return SendAsync<TResponse>(new ApiRequest(endpoint, HttpMethod.GET), cancellationToken);
        }

        public Task<ApiResult<TResponse>> PostAsync<TResponse>(string endpoint,
                                                               object body,
                                                               CancellationToken cancellationToken = default)
        {
            return SendWithBodyAsync<TResponse>(endpoint, HttpMethod.POST, body, cancellationToken);
        }

        public Task<ApiResult<TResponse>> PutAsync<TResponse>(string endpoint,
                                                              object body,
                                                              CancellationToken cancellationToken = default)
        {
            return SendWithBodyAsync<TResponse>(endpoint, HttpMethod.PUT, body, cancellationToken);
        }

        public Task<ApiResult<TResponse>> PatchAsync<TResponse>(string endpoint,
                                                                object body,
                                                                CancellationToken cancellationToken = default)
        {
            return SendWithBodyAsync<TResponse>(endpoint, HttpMethod.PATCH, body, cancellationToken);
        }

        public Task<ApiResult<TResponse>> DeleteAsync<TResponse>(string endpoint,
                                                                 CancellationToken cancellationToken = default)
        {
            return SendAsync<TResponse>(new ApiRequest(endpoint, HttpMethod.DELETE), cancellationToken);
        }

        private Task<ApiResult<TResponse>> SendWithBodyAsync<TResponse>(string endpoint,
                                                                        HttpMethod method,
                                                                        object body,
                                                                        CancellationToken cancellationToken)
        {
            ApiRequest request = new ApiRequest(endpoint, method)
            {
                    Body = body
            };

            return SendAsync<TResponse>(request, cancellationToken);
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
    }
}
