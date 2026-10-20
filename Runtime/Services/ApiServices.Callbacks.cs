using System;
using System.Threading;
using Deucarian.API.Configuration;
using Deucarian.API.Models;
using Deucarian.API.Core;

namespace Deucarian.API.Services
{
    /// <summary>Callback entry points for the configured legacy facade.</summary>
    public static partial class ApiServices
    {
        #region Methods

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Send<TResponse>(ApiRequest request,
                                           Action<TResponse> onSuccess,
                                           Action<ApiError> onFailure,
                                           CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => SendAsync<TResponse>(request, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Send<TResponse>(ApiEndpoint endpoint,
                                           Action<TResponse> onSuccess,
                                           Action<ApiError> onFailure,
                                           CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => SendAsync<TResponse>(endpoint, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Send<TResponse>(ApiEndpoint endpoint,
                                           object body,
                                           Action<TResponse> onSuccess,
                                           Action<ApiError> onFailure,
                                           CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => SendAsync<TResponse>(endpoint, body, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Send<TResponse>(ApiEndpointDefinition endpointDefinition,
                                           Action<TResponse> onSuccess,
                                           Action<ApiError> onFailure,
                                           CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => SendAsync<TResponse>(endpointDefinition, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Send<TResponse>(ApiEndpointDefinition endpointDefinition,
                                           object body,
                                           Action<TResponse> onSuccess,
                                           Action<ApiError> onFailure,
                                           CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => SendAsync<TResponse>(endpointDefinition, body, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Get<TResponse>(string endpoint,
                                          Action<TResponse> onSuccess,
                                          Action<ApiError> onFailure,
                                          CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => GetAsync<TResponse>(endpoint, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Post<TResponse>(string endpoint,
                                           object body,
                                           Action<TResponse> onSuccess,
                                           Action<ApiError> onFailure,
                                           CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => PostAsync<TResponse>(endpoint, body, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Put<TResponse>(string endpoint,
                                          object body,
                                          Action<TResponse> onSuccess,
                                          Action<ApiError> onFailure,
                                          CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => PutAsync<TResponse>(endpoint, body, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Patch<TResponse>(string endpoint,
                                            object body,
                                            Action<TResponse> onSuccess,
                                            Action<ApiError> onFailure,
                                            CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => PatchAsync<TResponse>(endpoint, body, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the request and posts exactly one completion on Unity's main thread.</summary>
        public static void Delete<TResponse>(string endpoint,
                                             Action<TResponse> onSuccess,
                                             Action<ApiError> onFailure,
                                             CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => DeleteAsync<TResponse>(endpoint, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        #endregion
    }
}
