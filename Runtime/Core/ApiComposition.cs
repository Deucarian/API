using System;
using System.Collections.Generic;
using Deucarian.API.Configuration;
using Deucarian.API.Models;

namespace Deucarian.API.Core
{
    /// <summary>Sanitized environment resolution state suitable for status UI.</summary>
    public sealed class ApiEnvironmentStatus
    {
        internal ApiEnvironmentStatus(ApiEnvironmentId environmentId,
                                      string displayName,
                                      bool isResolved,
                                      string message)
        {
            EnvironmentId = environmentId;
            DisplayName = displayName ?? string.Empty;
            IsResolved = isResolved;
            Message = message;
        }

        /// <summary>Requested environment identifier, or empty when the supplied value was invalid.</summary>
        public ApiEnvironmentId EnvironmentId { get; }

        /// <summary>Safe display label; empty when the supplied identifier was invalid.</summary>
        public string DisplayName { get; }

        /// <summary>True when the environment exists in the composition.</summary>
        public bool IsResolved { get; }

        /// <summary>Safe diagnostic message for unresolved state, otherwise null.</summary>
        public string Message { get; }
    }

    /// <summary>A named client resolved for one environment.</summary>
    public sealed class ApiResolvedClient
    {
        internal ApiResolvedClient(ApiEnvironmentId environmentId,
                                   string environmentDisplayName,
                                   ApiClientId clientId,
                                   string baseUrl,
                                   IDictionary<string, string> defaultHeaders,
                                   ApiRequestPolicy requestPolicy)
        {
            EnvironmentId = environmentId;
            EnvironmentDisplayName = environmentDisplayName ?? environmentId.Value;
            ClientId = clientId;
            BaseUrl = baseUrl;
            DefaultHeaders = new Dictionary<string, string>(
                defaultHeaders ?? new Dictionary<string, string>(),
                StringComparer.OrdinalIgnoreCase);
            RequestPolicy = requestPolicy ?? ApiRequestPolicy.Default;
        }

        /// <summary>Environment used for this resolution.</summary>
        public ApiEnvironmentId EnvironmentId { get; }

        /// <summary>Human-friendly environment label.</summary>
        public string EnvironmentDisplayName { get; }

        /// <summary>Resolved named-client identifier.</summary>
        public ApiClientId ClientId { get; }

        /// <summary>Resolved absolute base URL. Do not expose this through generic status UI.</summary>
        public string BaseUrl { get; }

        /// <summary>Resolved non-secret client headers.</summary>
        public IReadOnlyDictionary<string, string> DefaultHeaders { get; }

        /// <summary>Policy after environment and client overlays.</summary>
        public ApiRequestPolicy RequestPolicy { get; }
    }

    /// <summary>A catalog endpoint composed with a selected environment and named client.</summary>
    public sealed class ApiResolvedEndpoint
    {
        internal ApiResolvedEndpoint(ApiCatalogId catalogId,
                                     ApiEndpointId endpointId,
                                     ApiResolvedClient client,
                                     ApiEndpoint endpoint,
                                     ApiRequestPolicy requestPolicy)
        {
            CatalogId = catalogId;
            EndpointId = endpointId;
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            RequestPolicy = requestPolicy ?? ApiRequestPolicy.Default;
        }

        /// <summary>Catalog containing the endpoint.</summary>
        public ApiCatalogId CatalogId { get; }

        /// <summary>Stable endpoint identifier.</summary>
        public ApiEndpointId EndpointId { get; }

        /// <summary>Named client resolved for the selected environment.</summary>
        public ApiResolvedClient Client { get; }

        /// <summary>Existing API endpoint model with a resolved absolute route.</summary>
        public ApiEndpoint Endpoint { get; }

        /// <summary>Policy after environment, client, and endpoint overlays.</summary>
        public ApiRequestPolicy RequestPolicy { get; }

        /// <summary>Creates an advanced request through the existing ApiEndpoint pipeline.</summary>
        public ApiRequest CreateRequest(object body = null)
        {
            return Endpoint.CreateRequest(body);
        }
    }

    /// <summary>
    /// Resolves logical environment and endpoint IDs into existing <see cref="ApiEndpoint"/> and
    /// <see cref="ApiRequest"/> models. Selection is supplied per call; no active global environment is stored.
    /// </summary>
    public sealed class ApiComposition
    {
        private readonly Dictionary<ApiEnvironmentId, ApiEnvironmentProfile> environments =
                new Dictionary<ApiEnvironmentId, ApiEnvironmentProfile>();
        private readonly ApiEndpointCatalog endpointCatalog;
        private readonly ApiCatalogId catalogId;

        /// <summary>Creates a composition from one environment and one endpoint catalog.</summary>
        public ApiComposition(ApiEnvironmentProfile environment, ApiEndpointCatalog endpointCatalog)
            : this(new[] { environment }, endpointCatalog)
        {
        }

        /// <summary>Creates a composition with explicit environment choices and one route catalog.</summary>
        public ApiComposition(IEnumerable<ApiEnvironmentProfile> environmentProfiles,
                              ApiEndpointCatalog endpointCatalog)
        {
            this.endpointCatalog = endpointCatalog ?? throw new ArgumentNullException(nameof(endpointCatalog));

            string validationMessage;
            if (!endpointCatalog.IsValid(out validationMessage))
            {
                throw new ArgumentException(validationMessage, nameof(endpointCatalog));
            }

            ApiCatalogId resolvedCatalogId;
            endpointCatalog.TryGetId(out resolvedCatalogId);
            catalogId = resolvedCatalogId;
            if (environmentProfiles == null)
            {
                throw new ArgumentNullException(nameof(environmentProfiles));
            }

            foreach (ApiEnvironmentProfile environment in environmentProfiles)
            {
                if (environment == null)
                {
                    throw new ArgumentException("Environment collection cannot contain null profiles.",
                                                nameof(environmentProfiles));
                }

                if (!environment.IsValid(out validationMessage))
                {
                    throw new ArgumentException(validationMessage, nameof(environmentProfiles));
                }

                ApiEnvironmentId environmentId;
                environment.TryGetId(out environmentId);
                if (environments.ContainsKey(environmentId))
                {
                    throw new ArgumentException("Duplicate environment ID '" + environmentId + "'.",
                                                nameof(environmentProfiles));
                }

                environments.Add(environmentId, environment);
            }

            if (environments.Count == 0)
            {
                throw new ArgumentException("At least one API environment profile is required.",
                                            nameof(environmentProfiles));
            }
        }

        /// <summary>Stable ID of the composed endpoint catalog.</summary>
        public ApiCatalogId CatalogId => catalogId;

        /// <summary>Returns sanitized environment state without exposing base URLs or headers.</summary>
        public ApiEnvironmentStatus GetEnvironmentStatus(ApiEnvironmentId environmentId)
        {
            ApiEnvironmentProfile profile;
            if (environments.TryGetValue(environmentId, out profile))
            {
                string displayName = string.IsNullOrWhiteSpace(profile.DisplayName)
                                             ? environmentId.Value
                                             : profile.DisplayName;
                return new ApiEnvironmentStatus(environmentId, displayName, true, null);
            }

            return new ApiEnvironmentStatus(
                environmentId,
                environmentId.Value,
                false,
                "Environment '" + environmentId + "' is not registered in this composition.");
        }

        /// <summary>String overload for integrations that persist an environment ID outside this package.</summary>
        public ApiEnvironmentStatus GetEnvironmentStatus(string environmentId)
        {
            ApiEnvironmentId parsedId;
            if (!ApiEnvironmentId.TryParse(environmentId, out parsedId))
            {
                return new ApiEnvironmentStatus(
                    default(ApiEnvironmentId),
                    string.Empty,
                    false,
                    "The selected environment ID is invalid.");
            }

            return GetEnvironmentStatus(parsedId);
        }

        /// <summary>Resolves one named client for an explicit environment.</summary>
        public bool TryResolveClient(ApiEnvironmentId environmentId,
                                     ApiClientId clientId,
                                     out ApiResolvedClient client,
                                     out string message)
        {
            ApiEnvironmentProfile environment;
            if (!environments.TryGetValue(environmentId, out environment))
            {
                client = null;
                message = "Environment '" + environmentId + "' is not registered.";
                return false;
            }

            ApiNamedClientDefinition definition;
            if (!environment.TryGetClient(clientId, out definition))
            {
                client = null;
                message = "Environment '" + environmentId + "' does not define client '" + clientId + "'.";
                return false;
            }

            ApiRequestPolicy clientPolicy;
            try
            {
                ApiRequestPolicy environmentPolicy =
                        (environment.DefaultRequestPolicy ?? new ApiRequestPolicyDefinition())
                        .Resolve(ApiRequestPolicy.Default);
                clientPolicy =
                        (definition.RequestPolicy ?? new ApiRequestPolicyDefinition())
                        .Resolve(environmentPolicy);
            }
            catch (InvalidOperationException exception)
            {
                client = null;
                message = "Client '" + clientId + "' has an incompatible request policy: "
                          + exception.Message;
                return false;
            }

            Dictionary<string, string> headers =
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            ApiClientConfig.AddPairsToDictionary(definition.DefaultHeaders, headers);

            string displayName = string.IsNullOrWhiteSpace(environment.DisplayName)
                                         ? environmentId.Value
                                         : environment.DisplayName;
            client = new ApiResolvedClient(environmentId,
                                           displayName,
                                           clientId,
                                           definition.BaseUrl.Trim().TrimEnd('/'),
                                           headers,
                                           clientPolicy);
            message = null;
            return true;
        }

        /// <summary>Resolves a named client or throws a descriptive exception.</summary>
        public ApiResolvedClient ResolveClient(ApiEnvironmentId environmentId, ApiClientId clientId)
        {
            ApiResolvedClient client;
            string message;
            if (!TryResolveClient(environmentId, clientId, out client, out message))
            {
                throw new InvalidOperationException(message);
            }

            return client;
        }

        /// <summary>Resolves a catalog endpoint for an explicit environment.</summary>
        public bool TryResolveEndpoint(ApiEnvironmentId environmentId,
                                       ApiEndpointId endpointId,
                                       out ApiResolvedEndpoint endpoint,
                                       out string message)
        {
            ApiEndpointCatalogEntry definition;
            if (!endpointCatalog.TryGetEndpoint(endpointId, out definition))
            {
                endpoint = null;
                message = "Catalog '" + catalogId + "' does not define endpoint '" + endpointId + "'.";
                return false;
            }

            ApiClientId clientId;
            ApiClientId.TryParse(definition.ClientId, out clientId);
            ApiResolvedClient client;
            if (!TryResolveClient(environmentId, clientId, out client, out message))
            {
                endpoint = null;
                return false;
            }

            ApiRequestPolicy policy;
            try
            {
                policy = (definition.RequestPolicy ?? new ApiRequestPolicyDefinition())
                        .Resolve(client.RequestPolicy);
            }
            catch (InvalidOperationException exception)
            {
                endpoint = null;
                message = "Endpoint '" + endpointId + "' has an incompatible request policy: "
                          + exception.Message;
                return false;
            }
            Dictionary<string, string> headers =
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> header in client.DefaultHeaders)
            {
                headers[header.Key] = header.Value;
            }
            ApiClientConfig.AddPairsToDictionary(definition.DefaultHeaders, headers);
            Dictionary<string, string> query = new Dictionary<string, string>(StringComparer.Ordinal);
            ApiClientConfig.AddPairsToDictionary(definition.DefaultQueryParameters, query);

            string absoluteRoute = client.BaseUrl + "/" + definition.RouteTemplate.Trim().TrimStart('/');
            ApiEndpoint apiEndpoint = new ApiEndpoint(
                absoluteRoute,
                definition.Method,
                definition.Authentication,
                policy.TimeoutSeconds,
                headers,
                query,
                definition.ResponseFormat,
                policy,
                definition.SuppressLogging);

            endpoint = new ApiResolvedEndpoint(catalogId, endpointId, client, apiEndpoint, policy);
            message = null;
            return true;
        }

        /// <summary>Resolves an endpoint or throws a descriptive exception.</summary>
        public ApiResolvedEndpoint ResolveEndpoint(ApiEnvironmentId environmentId,
                                                   ApiEndpointId endpointId)
        {
            ApiResolvedEndpoint endpoint;
            string message;
            if (!TryResolveEndpoint(environmentId, endpointId, out endpoint, out message))
            {
                throw new InvalidOperationException(message);
            }

            return endpoint;
        }

        /// <summary>String overload for integrations that keep only stable ID strings.</summary>
        public ApiResolvedEndpoint ResolveEndpoint(string environmentId, string endpointId)
        {
            return ResolveEndpoint(new ApiEnvironmentId(environmentId), new ApiEndpointId(endpointId));
        }
    }
}
