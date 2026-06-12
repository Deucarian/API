using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Models;

namespace Deucarian.API.Core
{
    /// <summary>
    /// Main API client abstraction. Depend on this interface in application services.
    /// Create an instance with <see cref="ApiClientFactory"/> or provide your own implementation in tests.
    /// </summary>
    /// <example>
    /// <code>
    /// ApiResult&lt;ProjectDto[]&gt; result =
    ///     await apiClient.GetAsync&lt;ProjectDto[]&gt;("projects", cancellationToken);
    /// </code>
    /// </example>
    public interface IApiClient
    {
        /// <summary>
        /// Sends an advanced request object.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="request">Request metadata, headers, query parameters, body, auth mode, and timeout.</param>
        /// <param name="cancellationToken">Token used to cancel the request before completion.</param>
        /// <returns>A structured result containing data on success or <see cref="ApiError"/> on failure.</returns>
        Task<ApiResult<TResponse>> SendAsync<TResponse>(ApiRequest request,
                                                        CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a code-defined endpoint without a request body.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="endpoint">Endpoint metadata such as path, method, auth mode, headers, query, and timeout.</param>
        /// <param name="cancellationToken">Token used to cancel the request before completion.</param>
        /// <returns>A structured result containing data on success or <see cref="ApiError"/> on failure.</returns>
        Task<ApiResult<TResponse>> SendAsync<TResponse>(ApiEndpoint endpoint,
                                                        CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a code-defined endpoint with a request body.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="endpoint">Endpoint metadata such as path, method, auth mode, headers, query, and timeout.</param>
        /// <param name="body">Request body serialized according to the created request's body format.</param>
        /// <param name="cancellationToken">Token used to cancel the request before completion.</param>
        /// <returns>A structured result containing data on success or <see cref="ApiError"/> on failure.</returns>
        Task<ApiResult<TResponse>> SendAsync<TResponse>(ApiEndpoint endpoint,
                                                        object body,
                                                        CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a GET using a raw string endpoint or route constant.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="endpoint">Relative endpoint path or absolute URL.</param>
        /// <param name="cancellationToken">Token used to cancel the request before completion.</param>
        /// <returns>A structured result containing data on success or <see cref="ApiError"/> on failure.</returns>
        Task<ApiResult<TResponse>> GetAsync<TResponse>(string endpoint,
                                                       CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a POST using a raw string endpoint or route constant.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="endpoint">Relative endpoint path or absolute URL.</param>
        /// <param name="body">Request body serialized as JSON.</param>
        /// <param name="cancellationToken">Token used to cancel the request before completion.</param>
        /// <returns>A structured result containing data on success or <see cref="ApiError"/> on failure.</returns>
        Task<ApiResult<TResponse>> PostAsync<TResponse>(string endpoint,
                                                        object body,
                                                        CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a PUT using a raw string endpoint or route constant.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="endpoint">Relative endpoint path or absolute URL.</param>
        /// <param name="body">Request body serialized as JSON.</param>
        /// <param name="cancellationToken">Token used to cancel the request before completion.</param>
        /// <returns>A structured result containing data on success or <see cref="ApiError"/> on failure.</returns>
        Task<ApiResult<TResponse>> PutAsync<TResponse>(string endpoint,
                                                       object body,
                                                       CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a PATCH using a raw string endpoint or route constant.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="endpoint">Relative endpoint path or absolute URL.</param>
        /// <param name="body">Request body serialized as JSON.</param>
        /// <param name="cancellationToken">Token used to cancel the request before completion.</param>
        /// <returns>A structured result containing data on success or <see cref="ApiError"/> on failure.</returns>
        Task<ApiResult<TResponse>> PatchAsync<TResponse>(string endpoint,
                                                         object body,
                                                         CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a DELETE using a raw string endpoint or route constant.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="endpoint">Relative endpoint path or absolute URL.</param>
        /// <param name="cancellationToken">Token used to cancel the request before completion.</param>
        /// <returns>A structured result containing data on success or <see cref="ApiError"/> on failure.</returns>
        Task<ApiResult<TResponse>> DeleteAsync<TResponse>(string endpoint,
                                                          CancellationToken cancellationToken = default);
    }
}
