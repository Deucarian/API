using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Authentication;
using Deucarian.API.Configuration;
using Deucarian.API.Core;
using Deucarian.API.Models;
using UnityEngine;

namespace Deucarian.API.Services
{
    /// <summary>
    /// Legacy and convenience facade over a configured <see cref="IApiClient"/>.
    /// Prefer injecting <see cref="IApiClient"/> into new services instead of relying on this static facade.
    /// </summary>
    /// <example>
    /// <code>
    /// ApiServices.Configure(apiClient);
    /// ApiResult&lt;ProjectDto[]&gt; result =
    ///     await ApiServices.GetAsync&lt;ProjectDto[]&gt;("projects", cancellationToken);
    /// </code>
    /// </example>
    public static class ApiServices
    {
        private const string LegacyMigrationMessage =
                "Use an injected IApiClient with string endpoints, ApiEndpoint, or ApiRequest. Configure auth through ApiClientConfig/IApiAuthProvider instead of passing requiresAuthentication/accessToken.";

        private static IApiClient client;

        /// <summary>
        /// Process-wide client used by this legacy/convenience facade.
        /// Prefer injecting <see cref="IApiClient"/> into services instead of reading this property.
        /// </summary>
        public static IApiClient Client => client ?? (client = ApiClientFactory.CreateDefault());

        /// <summary>
        /// Replaces the facade client. Prefer dependency injection for new code.
        /// </summary>
        /// <param name="apiClient">Client used by all facade methods.</param>
        public static void Configure(IApiClient apiClient)
        {
            client = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        /// <summary>
        /// Creates and stores a facade client from config. Prefer dependency injection for new code.
        /// </summary>
        /// <param name="config">Config used to create the facade client.</param>
        /// <param name="authProviderOverride">Optional code auth provider that overrides the config asset provider.</param>
        public static void Configure(ApiClientConfig config, IApiAuthProvider authProviderOverride = null)
        {
            client = ApiClientFactory.Create(config, authProviderOverride);
        }

        /// <summary>
        /// Sends an advanced request through the configured facade client.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="request">Advanced request model.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>A structured API result.</returns>
        public static Task<ApiResult<TResponse>> SendAsync<TResponse>(
                ApiRequest request,
                CancellationToken cancellationToken = default)
        {
            return Client.SendAsync<TResponse>(request, cancellationToken);
        }

        /// <summary>
        /// Sends a code-defined endpoint through the configured facade client.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="endpoint">Code-defined endpoint.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>A structured API result.</returns>
        public static Task<ApiResult<TResponse>> SendAsync<TResponse>(
                ApiEndpoint endpoint,
                CancellationToken cancellationToken = default)
        {
            return Client.SendAsync<TResponse>(endpoint, cancellationToken);
        }

        /// <summary>
        /// Sends a code-defined endpoint with a body through the configured facade client.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="endpoint">Code-defined endpoint.</param>
        /// <param name="body">Request body.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>A structured API result.</returns>
        public static Task<ApiResult<TResponse>> SendAsync<TResponse>(
                ApiEndpoint endpoint,
                object body,
                CancellationToken cancellationToken = default)
        {
            return Client.SendAsync<TResponse>(endpoint, body, cancellationToken);
        }

        /// <summary>
        /// Sends a ScriptableObject endpoint through the configured facade client.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="endpointDefinition">ScriptableObject endpoint definition.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>A structured API result.</returns>
        public static Task<ApiResult<TResponse>> SendAsync<TResponse>(
                ApiEndpointDefinition endpointDefinition,
                CancellationToken cancellationToken = default)
        {
            if (endpointDefinition == null)
            {
                throw new ArgumentNullException(nameof(endpointDefinition));
            }

            return Client.SendAsync<TResponse>(endpointDefinition.ToEndpoint(), cancellationToken);
        }

        /// <summary>
        /// Sends a ScriptableObject endpoint with a body through the configured facade client.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="endpointDefinition">ScriptableObject endpoint definition.</param>
        /// <param name="body">Request body.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>A structured API result.</returns>
        public static Task<ApiResult<TResponse>> SendAsync<TResponse>(
                ApiEndpointDefinition endpointDefinition,
                object body,
                CancellationToken cancellationToken = default)
        {
            if (endpointDefinition == null)
            {
                throw new ArgumentNullException(nameof(endpointDefinition));
            }

            return Client.SendAsync<TResponse>(endpointDefinition.ToEndpoint(), body, cancellationToken);
        }

        /// <summary>
        /// Sends a GET using a raw string endpoint or route constant.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="endpoint">Relative endpoint path or absolute URL.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>A structured API result.</returns>
        public static Task<ApiResult<TResponse>> GetAsync<TResponse>(
                string endpoint,
                CancellationToken cancellationToken = default)
        {
            return Client.GetAsync<TResponse>(endpoint, cancellationToken);
        }

        /// <summary>
        /// Sends a POST using a raw string endpoint or route constant.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="endpoint">Relative endpoint path or absolute URL.</param>
        /// <param name="body">Request body serialized as JSON.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>A structured API result.</returns>
        public static Task<ApiResult<TResponse>> PostAsync<TResponse>(
                string endpoint,
                object body,
                CancellationToken cancellationToken = default)
        {
            return Client.PostAsync<TResponse>(endpoint, body, cancellationToken);
        }

        /// <summary>
        /// Sends a PUT using a raw string endpoint or route constant.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="endpoint">Relative endpoint path or absolute URL.</param>
        /// <param name="body">Request body serialized as JSON.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>A structured API result.</returns>
        public static Task<ApiResult<TResponse>> PutAsync<TResponse>(
                string endpoint,
                object body,
                CancellationToken cancellationToken = default)
        {
            return Client.PutAsync<TResponse>(endpoint, body, cancellationToken);
        }

        /// <summary>
        /// Sends a PATCH using a raw string endpoint or route constant.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="endpoint">Relative endpoint path or absolute URL.</param>
        /// <param name="body">Request body serialized as JSON.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>A structured API result.</returns>
        public static Task<ApiResult<TResponse>> PatchAsync<TResponse>(
                string endpoint,
                object body,
                CancellationToken cancellationToken = default)
        {
            return Client.PatchAsync<TResponse>(endpoint, body, cancellationToken);
        }

        /// <summary>
        /// Sends a DELETE using a raw string endpoint or route constant.
        /// </summary>
        /// <typeparam name="TResponse">The expected response DTO type.</typeparam>
        /// <param name="endpoint">Relative endpoint path or absolute URL.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>A structured API result.</returns>
        public static Task<ApiResult<TResponse>> DeleteAsync<TResponse>(
                string endpoint,
                CancellationToken cancellationToken = default)
        {
            return Client.DeleteAsync<TResponse>(endpoint, cancellationToken);
        }

        /// <summary>
        /// Legacy multipart POST overload. Use <see cref="IApiClient.SendAsync{TResponse}(ApiRequest, CancellationToken)"/>
        /// with <see cref="ApiRequestBodyFormat.MultipartForm"/> for new code.
        /// </summary>
        [Obsolete(LegacyMigrationMessage)]
        public static Task<ApiCallResult<TResponse>> PostMultipartAsync<TResponse>(
                string endpoint,
                object data,
                bool requiresAuthentication,
                string accessToken = null,
                [CallerMemberName] string callerMemberName = "",
                [CallerFilePath] string callerFilePath = "",
                [CallerLineNumber] int callerLineNumber = 0)
        {
            return ExecuteLegacyAsync<TResponse>(endpoint,
                                                 HttpMethod.POST,
                                                 requiresAuthentication,
                                                 data,
                                                 accessToken,
                                                 null,
                                                 ApiRequestBodyFormat.MultipartForm,
                                                 CancellationToken.None);
        }

        /// <summary>
        /// Legacy POST overload. Use an injected <see cref="IApiClient"/> and configure authentication through
        /// <see cref="ApiClientConfig"/> and <see cref="IApiAuthProvider"/>.
        /// </summary>
        [Obsolete(LegacyMigrationMessage)]
        public static Task<ApiCallResult<TResponse>> PostAsync<TResponse>(
                string endpoint,
                object data,
                bool requiresAuthentication,
                string accessToken = null,
                [CallerMemberName] string callerMemberName = "",
                [CallerFilePath] string callerFilePath = "",
                [CallerLineNumber] int callerLineNumber = 0)
        {
            return ExecuteLegacyAsync<TResponse>(endpoint,
                                                 HttpMethod.POST,
                                                 requiresAuthentication,
                                                 data,
                                                 accessToken,
                                                 null,
                                                 ApiRequestBodyFormat.Json,
                                                 CancellationToken.None);
        }

        /// <summary>
        /// Legacy PUT overload. Use an injected <see cref="IApiClient"/> and configure authentication through
        /// <see cref="ApiClientConfig"/> and <see cref="IApiAuthProvider"/>.
        /// </summary>
        [Obsolete(LegacyMigrationMessage)]
        public static Task<ApiCallResult<TResponse>> PutAsync<TResponse>(
                string endpoint,
                object data,
                bool requiresAuthentication,
                string accessToken = null,
                [CallerMemberName] string callerMemberName = "",
                [CallerFilePath] string callerFilePath = "",
                [CallerLineNumber] int callerLineNumber = 0)
        {
            return ExecuteLegacyAsync<TResponse>(endpoint,
                                                 HttpMethod.PUT,
                                                 requiresAuthentication,
                                                 data,
                                                 accessToken,
                                                 null,
                                                 ApiRequestBodyFormat.Json,
                                                 CancellationToken.None);
        }

        /// <summary>
        /// Legacy PATCH overload. Use an injected <see cref="IApiClient"/> and configure authentication through
        /// <see cref="ApiClientConfig"/> and <see cref="IApiAuthProvider"/>.
        /// </summary>
        [Obsolete(LegacyMigrationMessage)]
        public static Task<ApiCallResult<TResponse>> PatchAsync<TResponse>(
                string endpoint,
                object data,
                bool requiresAuthentication,
                string accessToken = null,
                [CallerMemberName] string callerMemberName = "",
                [CallerFilePath] string callerFilePath = "",
                [CallerLineNumber] int callerLineNumber = 0)
        {
            return ExecuteLegacyAsync<TResponse>(endpoint,
                                                 HttpMethod.PATCH,
                                                 requiresAuthentication,
                                                 data,
                                                 accessToken,
                                                 null,
                                                 ApiRequestBodyFormat.Json,
                                                 CancellationToken.None);
        }

        /// <summary>
        /// Legacy GET overload. Use an injected <see cref="IApiClient"/> and configure authentication through
        /// <see cref="ApiClientConfig"/> and <see cref="IApiAuthProvider"/>.
        /// </summary>
        [Obsolete(LegacyMigrationMessage)]
        public static Task<ApiCallResult<TResponse>> GetAsync<TResponse>(
                string endpoint,
                bool requiresAuthentication,
                string accessToken = null,
                Dictionary<string, string> customHeaders = null,
                [CallerMemberName] string callerMemberName = "",
                [CallerFilePath] string callerFilePath = "",
                [CallerLineNumber] int callerLineNumber = 0)
        {
            return ExecuteLegacyAsync<TResponse>(endpoint,
                                                 HttpMethod.GET,
                                                 requiresAuthentication,
                                                 null,
                                                 accessToken,
                                                 customHeaders,
                                                 ApiRequestBodyFormat.Json,
                                                 CancellationToken.None);
        }

        /// <summary>
        /// Legacy DELETE overload. Use an injected <see cref="IApiClient"/> and configure authentication through
        /// <see cref="ApiClientConfig"/> and <see cref="IApiAuthProvider"/>.
        /// </summary>
        [Obsolete(LegacyMigrationMessage)]
        public static Task<ApiCallResult<TResponse>> DeleteAsync<TResponse>(
                string endpoint,
                bool requiresAuthentication,
                string accessToken = null,
                [CallerMemberName] string callerMemberName = "",
                [CallerFilePath] string callerFilePath = "",
                [CallerLineNumber] int callerLineNumber = 0)
        {
            return ExecuteLegacyAsync<TResponse>(endpoint,
                                                 HttpMethod.DELETE,
                                                 requiresAuthentication,
                                                 null,
                                                 accessToken,
                                                 null,
                                                 ApiRequestBodyFormat.Json,
                                                 CancellationToken.None);
        }

        /// <summary>
        /// Legacy texture download helper retained for compatibility.
        /// Prefer <see cref="IApiClient.GetAsync{TResponse}(string, CancellationToken)"/> with <see cref="Texture2D"/>.
        /// </summary>
        [Obsolete(LegacyMigrationMessage)]
        public static Task<ApiCallResult<Texture2D>> DownloadTexture2DAsync(
                string endpoint,
                string accessToken = null,
                Dictionary<string, string> customHeaders = null,
                CancellationToken cancellationToken = default)
        {
            TextureDownloadService service = new TextureDownloadService();
            return service.ExecuteAsync(endpoint: endpoint,
                                        accessToken: accessToken,
                                        customHeaders: customHeaders,
                                        cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Legacy byte download helper retained for compatibility.
        /// Prefer <see cref="IApiClient.GetAsync{TResponse}(string, CancellationToken)"/> with <see cref="byte"/>[].
        /// </summary>
        [Obsolete(LegacyMigrationMessage)]
        public static Task<ApiCallResult<byte[]>> DownloadBytesAsync(
                string endpoint,
                string accessToken = null,
                Dictionary<string, string> customHeaders = null,
                CancellationToken cancellationToken = default)
        {
            BinaryDownloadService service = new BinaryDownloadService();
            return service.ExecuteAsync(endpoint: endpoint,
                                        accessToken: accessToken,
                                        customHeaders: customHeaders,
                                        cancellationToken: cancellationToken);
        }

        internal static async Task<ApiCallResult<TResponse>> ExecuteLegacyAsync<TResponse>(
                string endpoint,
                HttpMethod method,
                bool requiresAuthentication,
                object data,
                string accessToken,
                Dictionary<string, string> customHeaders,
                ApiRequestBodyFormat bodyFormat,
                CancellationToken cancellationToken)
        {
            ApiRequest request = new ApiRequest(endpoint,
                                                method,
                                                requiresAuthentication
                                                        ? ApiAuthenticationRequirement.Required
                                                        : ApiAuthenticationRequirement.Disabled)
            {
                    Body = data,
                    BearerTokenOverride = accessToken,
                    BodyFormat = bodyFormat
            };

            if (customHeaders != null)
            {
                foreach (KeyValuePair<string, string> header in customHeaders)
                {
                    request.Headers[header.Key] = header.Value;
                }
            }

            ApiResult<TResponse> result = await Client.SendAsync<TResponse>(request, cancellationToken);
            return ToApiCallResult(result);
        }

        internal static ApiCallResult<TResponse> ToApiCallResult<TResponse>(ApiResult<TResponse> result)
        {
            return ApiCallResult<TResponse>.From(result);
        }
    }
}
