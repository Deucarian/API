using System.Collections.Generic;
using Deucarian.API.Authentication;
using Deucarian.API.Models;
using UnityEngine;

namespace Deucarian.API.Configuration
{
    /// <summary>
    /// ScriptableObject-backed configuration for creating an <c>IApiClient</c>.
    /// Create one from Assets/Create/Deucarian/API/Client Config, then pass it to
    /// <c>ApiClientFactory.Create(config)</c>.
    /// </summary>
    /// <example>
    /// <code>
    /// ApiClientConfig config = ApiClientConfig.CreateRuntimeDefault();
    /// config.BaseUrl = "https://api.example.com";
    /// IApiClient apiClient = ApiClientFactory.Create(config);
    /// </code>
    /// </example>
    [CreateAssetMenu(menuName = "Deucarian/API/Client Config", fileName = "ApiClientConfig")]
    public sealed class ApiClientConfig : ScriptableObject
    {
        private const int DefaultTimeoutSeconds = 30;

        [Tooltip("Base URL prepended to relative endpoints, for example 'https://api.example.com/api/v2'.")]
        [SerializeField] private string baseUrl;

        [Tooltip("Headers applied to every request unless overridden by the request or endpoint.")]
        [SerializeField] private List<ApiKeyValuePair> defaultHeaders = new List<ApiKeyValuePair>();

        [Tooltip("Default request timeout in seconds. Set 0 to let UnityWebRequest use its default behavior.")]
        [SerializeField] private int timeoutSeconds = DefaultTimeoutSeconds;

        [Tooltip("Default authentication behavior for requests that use Config Default.")]
        [SerializeField] private ApiAuthenticationMode authenticationMode = ApiAuthenticationMode.None;

        [Tooltip("Optional ScriptableObject token provider used when Bearer Token auth is enabled.")]
        [SerializeField] private ApiAuthProviderAsset authProvider;

        [Tooltip("Newtonsoft JSON serialization settings used for request and response bodies.")]
        [SerializeField] private ApiJsonSerializerOptions jsonSerializerSettings =
                new ApiJsonSerializerOptions();

        [Tooltip("Default response format for DTO-like calls. Keep Auto for most projects; use per-request or per-endpoint overrides for text, bytes, and textures.")]
        [SerializeField] private ApiResponseFormat defaultResponseFormat = ApiResponseFormat.Auto;

        [Tooltip("Certificate validation mode. Default Validation is the safe production default.")]
        [SerializeField] private ApiCertificateHandlingMode certificateHandlingMode =
                ApiCertificateHandlingMode.DefaultValidation;

        [Tooltip("Controls API request/response logging.")]
        [SerializeField] private ApiLoggingMode loggingMode = ApiLoggingMode.ErrorsOnly;

        [Tooltip("Logs successful JSON response bodies when logging is enabled.")]
        [SerializeField] private bool logRawJson;

        /// <summary>
        /// Base URL prepended to relative endpoints. Absolute URLs skip this value.
        /// </summary>
        public string BaseUrl
        {
            get => baseUrl;
            set => baseUrl = value;
        }

        /// <summary>
        /// Headers applied to every request unless overridden by an endpoint or request.
        /// </summary>
        public List<ApiKeyValuePair> DefaultHeaders => defaultHeaders;

        /// <summary>
        /// Default timeout in seconds. Set 0 to let UnityWebRequest use its default behavior.
        /// </summary>
        public int TimeoutSeconds
        {
            get => timeoutSeconds;
            set => timeoutSeconds = value < 0 ? 0 : value;
        }

        /// <summary>
        /// Default authentication mode used by requests/endpoints set to UseConfigDefault.
        /// </summary>
        public ApiAuthenticationMode AuthenticationMode
        {
            get => authenticationMode;
            set => authenticationMode = value;
        }

        /// <summary>
        /// Optional ScriptableObject auth provider used for bearer token requests.
        /// </summary>
        public ApiAuthProviderAsset AuthProvider
        {
            get => authProvider;
            set => authProvider = value;
        }

        /// <summary>
        /// Newtonsoft JSON serializer options used by the default client.
        /// </summary>
        public ApiJsonSerializerOptions JsonSerializerSettings
        {
            get => jsonSerializerSettings;
            set => jsonSerializerSettings = value ?? new ApiJsonSerializerOptions();
        }

        /// <summary>
        /// Default response format used when an ApiRequest/ApiEndpoint leaves ResponseFormat on Auto.
        /// Keep this on Auto for most projects. Auto keeps JSON as the DTO default while still
        /// inferring string, byte[], and Texture2D. Non-Auto values apply globally to DTO-like
        /// responses whose request/endpoint format is Auto, so prefer per-request or per-endpoint
        /// overrides for text, bytes, and textures.
        /// </summary>
        public ApiResponseFormat DefaultResponseFormat
        {
            get => defaultResponseFormat;
            set => defaultResponseFormat = value;
        }

        /// <summary>
        /// Certificate validation behavior. DefaultValidation is the safe production default.
        /// </summary>
        public ApiCertificateHandlingMode CertificateHandlingMode
        {
            get => certificateHandlingMode;
            set => certificateHandlingMode = value;
        }

        /// <summary>
        /// Controls request, response, and error logging.
        /// </summary>
        public ApiLoggingMode LoggingMode
        {
            get => loggingMode;
            set => loggingMode = value;
        }

        /// <summary>
        /// Logs successful JSON response bodies when logging is enabled.
        /// </summary>
        public bool LogRawJson
        {
            get => logRawJson;
            set => logRawJson = value;
        }

        /// <summary>
        /// Returns config default headers as a dictionary.
        /// </summary>
        /// <returns>A copy of the configured default headers.</returns>
        public Dictionary<string, string> GetDefaultHeaderDictionary()
        {
            Dictionary<string, string> headers =
                    new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            AddPairsToDictionary(defaultHeaders, headers);
            return headers;
        }

        /// <summary>
        /// Creates a runtime-only config instance with safe defaults.
        /// </summary>
        /// <returns>A new ScriptableObject config instance.</returns>
        public static ApiClientConfig CreateRuntimeDefault()
        {
            return CreateInstance<ApiClientConfig>();
        }

        internal static void AddPairsToDictionary(IEnumerable<ApiKeyValuePair> pairs,
                                                  IDictionary<string, string> destination)
        {
            if (pairs == null || destination == null)
            {
                return;
            }

            foreach (ApiKeyValuePair pair in pairs)
            {
                if (pair == null || string.IsNullOrWhiteSpace(pair.Key))
                {
                    continue;
                }

                destination[pair.Key] = pair.Value;
            }
        }
    }
}
