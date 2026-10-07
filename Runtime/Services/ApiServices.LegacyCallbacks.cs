using System;
using System.Collections.Generic;
using System.Threading;
using Deucarian.API.Core;
using Deucarian.API.Models;
using UnityEngine;

namespace Deucarian.API.Services
{
    public static partial class ApiServices
    {
        #region Methods

        /// <summary>Legacy request shape with typed success/failure callbacks and cancellation.</summary>
        [Obsolete(LegacyMigrationMessage)]
        public static void Get<TResponse>(string endpoint,
                                          bool requiresAuthentication,
                                          Action<TResponse> onSuccess,
                                          Action<ApiError> onFailure,
                                          string accessToken = null,
                                          Dictionary<string, string> customHeaders = null,
                                          CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run<TResponse>(async () =>
                                        {
                                            return await ExecuteLegacyAsync<TResponse>(endpoint,
                                                                                       HttpMethod.GET,
                                                                                       requiresAuthentication,
                                                                                       null,
                                                                                       accessToken,
                                                                                       customHeaders,
                                                                                       ApiRequestBodyFormat.Json,
                                                                                       cancellationToken
                                            );
                                        },
                                        onSuccess,
                                        onFailure,
                                        cancellationToken
            );
        }

        /// <summary>Legacy request shape with typed success/failure callbacks and cancellation.</summary>
        [Obsolete(LegacyMigrationMessage)]
        public static void Post<TResponse>(string endpoint,
                                           object data,
                                           bool requiresAuthentication,
                                           Action<TResponse> onSuccess,
                                           Action<ApiError> onFailure,
                                           string accessToken = null,
                                           CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run<TResponse>(async () =>
                                        {
                                            return await ExecuteLegacyAsync<TResponse>(endpoint,
                                                                                       HttpMethod.POST,
                                                                                       requiresAuthentication,
                                                                                       data,
                                                                                       accessToken,
                                                                                       null,
                                                                                       ApiRequestBodyFormat.Json,
                                                                                       cancellationToken
                                            );
                                        },
                                        onSuccess,
                                        onFailure,
                                        cancellationToken
            );
        }

        /// <summary>Legacy request shape with typed success/failure callbacks and cancellation.</summary>
        [Obsolete(LegacyMigrationMessage)]
        public static void Put<TResponse>(string endpoint,
                                          object data,
                                          bool requiresAuthentication,
                                          Action<TResponse> onSuccess,
                                          Action<ApiError> onFailure,
                                          string accessToken = null,
                                          CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run<TResponse>(async () =>
                                        {
                                            return await ExecuteLegacyAsync<TResponse>(endpoint,
                                                                                       HttpMethod.PUT,
                                                                                       requiresAuthentication,
                                                                                       data,
                                                                                       accessToken,
                                                                                       null,
                                                                                       ApiRequestBodyFormat.Json,
                                                                                       cancellationToken
                                            );
                                        },
                                        onSuccess,
                                        onFailure,
                                        cancellationToken
            );
        }

        /// <summary>Legacy request shape with typed success/failure callbacks and cancellation.</summary>
        [Obsolete(LegacyMigrationMessage)]
        public static void Patch<TResponse>(string endpoint,
                                            object data,
                                            bool requiresAuthentication,
                                            Action<TResponse> onSuccess,
                                            Action<ApiError> onFailure,
                                            string accessToken = null,
                                            CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run<TResponse>(async () =>
                                        {
                                            return await ExecuteLegacyAsync<TResponse>(endpoint,
                                                                                       HttpMethod.PATCH,
                                                                                       requiresAuthentication,
                                                                                       data,
                                                                                       accessToken,
                                                                                       null,
                                                                                       ApiRequestBodyFormat.Json,
                                                                                       cancellationToken
                                            );
                                        },
                                        onSuccess,
                                        onFailure,
                                        cancellationToken
            );
        }

        /// <summary>Legacy request shape with typed success/failure callbacks and cancellation.</summary>
        [Obsolete(LegacyMigrationMessage)]
        public static void Delete<TResponse>(string endpoint,
                                             bool requiresAuthentication,
                                             Action<TResponse> onSuccess,
                                             Action<ApiError> onFailure,
                                             string accessToken = null,
                                             CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run<TResponse>(async () =>
                                        {
                                            return await ExecuteLegacyAsync<TResponse>(endpoint,
                                                                                       HttpMethod.DELETE,
                                                                                       requiresAuthentication,
                                                                                       null,
                                                                                       accessToken,
                                                                                       null,
                                                                                       ApiRequestBodyFormat.Json,
                                                                                       cancellationToken
                                            );
                                        },
                                        onSuccess,
                                        onFailure,
                                        cancellationToken
            );
        }

        /// <summary>Legacy request shape with typed success/failure callbacks and cancellation.</summary>
        [Obsolete(LegacyMigrationMessage)]
        public static void PostMultipart<TResponse>(string endpoint,
                                                    object data,
                                                    bool requiresAuthentication,
                                                    Action<TResponse> onSuccess,
                                                    Action<ApiError> onFailure,
                                                    string accessToken = null,
                                                    CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run<TResponse>(async () =>
                                        {
                                            return await ExecuteLegacyAsync<TResponse>(endpoint,
                                                                                       HttpMethod.POST,
                                                                                       requiresAuthentication,
                                                                                       data,
                                                                                       accessToken,
                                                                                       null,
                                                                                       ApiRequestBodyFormat
                                                                                           .MultipartForm,
                                                                                       cancellationToken
                                            );
                                        },
                                        onSuccess,
                                        onFailure,
                                        cancellationToken
            );
        }

        /// <summary>Downloads media with typed success/failure callbacks on Unity's main thread.</summary>
        [Obsolete(LegacyMigrationMessage)]
        public static void DownloadTexture2D(string endpoint,
                                             Action<Texture2D> onSuccess,
                                             Action<ApiError> onFailure,
                                             string accessToken = null,
                                             Dictionary<string, string> customHeaders = null,
                                             CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run<Texture2D>(async () =>
                                        {
                                            TextureDownloadService service = new TextureDownloadService();
                                            return await service.ExecuteAsync(endpoint,
                                                                              accessToken,
                                                                              customHeaders,
                                                                              cancellationToken
                                            );
                                        },
                                        onSuccess,
                                        onFailure,
                                        cancellationToken
            );
        }

        /// <summary>Downloads media with typed success/failure callbacks on Unity's main thread.</summary>
        [Obsolete(LegacyMigrationMessage)]
        public static void DownloadBytes(string endpoint,
                                         Action<byte[]> onSuccess,
                                         Action<ApiError> onFailure,
                                         string accessToken = null,
                                         Dictionary<string, string> customHeaders = null,
                                         CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run<byte[]>(async () =>
                                     {
                                         BinaryDownloadService service = new BinaryDownloadService();
                                         return await service.ExecuteAsync(endpoint,
                                                                           accessToken,
                                                                           customHeaders,
                                                                           cancellationToken
                                         );
                                     },
                                     onSuccess,
                                     onFailure,
                                     cancellationToken
            );
        }

        #endregion
    }
}
