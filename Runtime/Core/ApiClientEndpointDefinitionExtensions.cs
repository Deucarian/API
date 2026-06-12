using System;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Configuration;
using Deucarian.API.Models;

namespace Deucarian.API.Core
{
    /// <summary>
    /// Convenience methods for using ScriptableObject endpoint definitions with any <see cref="IApiClient"/>.
    /// </summary>
    public static class ApiClientEndpointDefinitionExtensions
    {
        /// <summary>
        /// Sends a ScriptableObject endpoint definition without a request body.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="apiClient">Client used to send the request.</param>
        /// <param name="endpointDefinition">Endpoint asset containing path, method, auth, headers, query, and timeout settings.</param>
        /// <param name="cancellationToken">Token used to cancel the request before completion.</param>
        /// <returns>A structured result containing data on success or <see cref="ApiError"/> on failure.</returns>
        public static Task<ApiResult<TResponse>> SendAsync<TResponse>(
                this IApiClient apiClient,
                ApiEndpointDefinition endpointDefinition,
                CancellationToken cancellationToken = default)
        {
            if (apiClient == null)
            {
                throw new ArgumentNullException(nameof(apiClient));
            }

            if (endpointDefinition == null)
            {
                throw new ArgumentNullException(nameof(endpointDefinition));
            }

            return apiClient.SendAsync<TResponse>(endpointDefinition.ToEndpoint(), cancellationToken);
        }

        /// <summary>
        /// Sends a ScriptableObject endpoint definition with a request body.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="apiClient">Client used to send the request.</param>
        /// <param name="endpointDefinition">Endpoint asset containing path, method, auth, headers, query, and timeout settings.</param>
        /// <param name="body">Request body serialized as JSON by the default client.</param>
        /// <param name="cancellationToken">Token used to cancel the request before completion.</param>
        /// <returns>A structured result containing data on success or <see cref="ApiError"/> on failure.</returns>
        public static Task<ApiResult<TResponse>> SendAsync<TResponse>(
                this IApiClient apiClient,
                ApiEndpointDefinition endpointDefinition,
                object body,
                CancellationToken cancellationToken = default)
        {
            if (apiClient == null)
            {
                throw new ArgumentNullException(nameof(apiClient));
            }

            if (endpointDefinition == null)
            {
                throw new ArgumentNullException(nameof(endpointDefinition));
            }

            return apiClient.SendAsync<TResponse>(endpointDefinition.ToEndpoint(), body, cancellationToken);
        }
    }
}
