using JorisHoef.APIHelper.Authentication;
using JorisHoef.APIHelper.Certificates;
using JorisHoef.APIHelper.Configuration;

namespace JorisHoef.APIHelper.Core
{
    /// <summary>
    /// Creates default <see cref="IApiClient"/> instances from APIHelper configuration.
    /// This is intentionally small; application composition can still provide or wrap <see cref="IApiClient"/> directly.
    /// </summary>
    /// <example>
    /// <code>
    /// IApiClient apiClient = ApiClientFactory.Create(apiClientConfig, authProvider);
    /// </code>
    /// </example>
    public static class ApiClientFactory
    {
        /// <summary>
        /// Creates an API client with safe runtime defaults.
        /// Prefer <see cref="Create(ApiClientConfig, IApiAuthProvider)"/> when a project config exists.
        /// </summary>
        /// <returns>A new API client using a runtime config with safe defaults.</returns>
        public static IApiClient CreateDefault()
        {
            return Create(ApiClientConfig.CreateRuntimeDefault());
        }

        /// <summary>
        /// Creates an <see cref="IApiClient"/> from a ScriptableObject-backed config.
        /// Pass an auth provider override when the token source is created in code instead of assigned on the config asset.
        /// </summary>
        /// <param name="config">ScriptableObject-backed client config. Null creates a runtime default config.</param>
        /// <param name="authProviderOverride">Optional code-defined auth provider that overrides the config asset provider.</param>
        /// <returns>A configured API client.</returns>
        public static IApiClient Create(ApiClientConfig config,
                                        IApiAuthProvider authProviderOverride = null)
        {
            ApiClientConfig effectiveConfig = config ?? ApiClientConfig.CreateRuntimeDefault();
            IApiSerializer serializer = new NewtonsoftApiSerializer(effectiveConfig.JsonSerializerSettings);
            IApiAuthProvider authProvider = authProviderOverride ?? effectiveConfig.AuthProvider;
            ICertificateHandlerFactory certificateFactory =
                    new ApiCertificateHandlerFactory(effectiveConfig.CertificateHandlingMode);

            IRequestBuilder requestBuilder =
                    new UnityWebRequestBuilder(effectiveConfig, serializer, authProvider, certificateFactory);
            IRequestSender requestSender = new UnityWebRequestSender();
            IApiResponseParser responseParser = new ApiResponseParser(serializer);
            IApiErrorParser errorParser = new ApiErrorParser();
            IApiLogger logger = new UnityApiLogger(effectiveConfig);

            return new ApiClient(requestBuilder,
                                 requestSender,
                                 responseParser,
                                 errorParser,
                                 logger,
                                 effectiveConfig.DefaultResponseFormat);
        }
    }
}
