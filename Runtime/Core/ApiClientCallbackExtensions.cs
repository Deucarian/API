using System;
using System.Threading;
using Deucarian.API.Configuration;
using Deucarian.API.Models;

namespace Deucarian.API.Core
{
    /// <summary>Typed success/failure callbacks for every IApiClient request shape.</summary>
    public static class ApiClientCallbackExtensions
    {
        #region Methods

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Send<TResponse>(this IApiClient apiClient,
                                           ApiRequest request,
                                           Action<TResponse> onSuccess,
                                           Action<ApiError> onFailure,
                                           CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => apiClient.SendAsync<TResponse>(request, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Send<TResponse>(this IApiClient apiClient,
                                           ApiEndpoint endpoint,
                                           Action<TResponse> onSuccess,
                                           Action<ApiError> onFailure,
                                           CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => apiClient.SendAsync<TResponse>(endpoint, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Send<TResponse>(this IApiClient apiClient,
                                           ApiEndpoint endpoint,
                                           object body,
                                           Action<TResponse> onSuccess,
                                           Action<ApiError> onFailure,
                                           CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => apiClient.SendAsync<TResponse>(endpoint, body, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Send<TResponse>(this IApiClient apiClient,
                                           ApiEndpointDefinition endpointDefinition,
                                           Action<TResponse> onSuccess,
                                           Action<ApiError> onFailure,
                                           CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => apiClient.SendAsync<TResponse>(endpointDefinition, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Send<TResponse>(this IApiClient apiClient,
                                           ApiEndpointDefinition endpointDefinition,
                                           object body,
                                           Action<TResponse> onSuccess,
                                           Action<ApiError> onFailure,
                                           CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => apiClient.SendAsync<TResponse>(endpointDefinition, body, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Get<TResponse>(this IApiClient apiClient,
                                          string endpoint,
                                          Action<TResponse> onSuccess,
                                          Action<ApiError> onFailure,
                                          CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => apiClient.GetAsync<TResponse>(endpoint, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Post<TResponse>(this IApiClient apiClient,
                                           string endpoint,
                                           object body,
                                           Action<TResponse> onSuccess,
                                           Action<ApiError> onFailure,
                                           CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => apiClient.PostAsync<TResponse>(endpoint, body, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Put<TResponse>(this IApiClient apiClient,
                                          string endpoint,
                                          object body,
                                          Action<TResponse> onSuccess,
                                          Action<ApiError> onFailure,
                                          CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => apiClient.PutAsync<TResponse>(endpoint, body, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Patch<TResponse>(this IApiClient apiClient,
                                            string endpoint,
                                            object body,
                                            Action<TResponse> onSuccess,
                                            Action<ApiError> onFailure,
                                            CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => apiClient.PatchAsync<TResponse>(endpoint, body, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Delete<TResponse>(this IApiClient apiClient,
                                             string endpoint,
                                             Action<TResponse> onSuccess,
                                             Action<ApiError> onFailure,
                                             CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => apiClient.DeleteAsync<TResponse>(endpoint, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        #endregion
    }
}
