using Deucarian.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Models;
using UnityEngine;

namespace Deucarian.API.Core
{
    /// <summary>Configured API access for one scope. Borrowed clients keep their original owner.</summary>
    [DisallowMultipleComponent]
    public sealed class ApiHost : MonoBehaviour, IDiagnosticProvider
    {
        private Dictionary<string, ApiEndpointBinding> bindings;
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private bool destroyed;

        public void Configure(IEnumerable<ApiEndpointBinding> endpoints)
        {
            if (destroyed) throw new ObjectDisposedException(nameof(ApiHost));
            if (bindings != null) throw new InvalidOperationException("ApiHost '" + name + "' is already configured. Configure each host once during startup.");
            if (endpoints == null) throw new ArgumentNullException(nameof(endpoints));
            var copy = new Dictionary<string, ApiEndpointBinding>(StringComparer.Ordinal);
            foreach (var endpoint in endpoints)
            {
                if (endpoint == null) throw new ArgumentException("An API endpoint binding is null. Supply a binding for each configured endpoint.", nameof(endpoints));
                if (copy.ContainsKey(endpoint.Id)) throw new ArgumentException("Duplicate endpoint '" + endpoint.Id + "'. Register each endpoint once in this ApiHost.", nameof(endpoints));
                copy.Add(endpoint.Id, endpoint);
            }
            bindings = copy;
        }

        public async Task<ApiResult<TResponse>> SendAsync<TRequest, TResponse>(EndpointKey<TRequest, TResponse> endpoint,
            TRequest request, CancellationToken cancellationToken = default)
        {
            if (destroyed) throw new ObjectDisposedException(nameof(ApiHost));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint), "Select an endpoint matching the request and response types, or pass its named EndpointKey.");
            if (bindings == null) throw new InvalidOperationException("ApiHost '" + name + "' is not configured. Call Configure once with your endpoint bindings during startup.");
            if (!bindings.TryGetValue(endpoint.Id, out var binding))
                throw new InvalidOperationException("ApiHost '" + name + "' has no binding for endpoint '" + endpoint.Id + "'. Add ApiEndpointBinding.For for this key to the host's configuration.");
            if (!(binding is ApiEndpointBinding.Typed<TRequest, TResponse> typed))
                throw new InvalidOperationException("Endpoint '" + endpoint.Id + "' is bound to different request or response types. Configure it using the same typed EndpointKey as the caller.");
            using (var cancellation = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token, cancellationToken))
            {
                cancellation.Token.ThrowIfCancellationRequested();
                var result = await typed.SendAsync(request, cancellation.Token);
                cancellation.Token.ThrowIfCancellationRequested();
                return result;
            }
        }

        private void OnDestroy()
        { diagnosticRegistration?.Dispose(); diagnosticRegistration = null;
            destroyed = true;
            lifetime.Cancel();
            lifetime.Dispose();
            bindings?.Clear();
            bindings = null;
        }
        private DiagnosticProviderRegistration diagnosticRegistration;
        private void Awake() => diagnosticRegistration = DiagnosticProviderRegistry.Register(this);
        string IDiagnosticProvider.ProviderId => "api.host." + GetInstanceID();
        string IDiagnosticProvider.DisplayName => "ApiHost";
        void IDiagnosticProvider.Collect(DiagnosticReportBuilder builder)
        {
            bool configured = bindings != null;
            builder.AddSection(((IDiagnosticProvider)this).ProviderId, "ApiHost")
                .AddItem("configured", "Configured", configured ? "Ready" : "Call Configure during startup",
                    configured ? DiagnosticSeverity.Info : DiagnosticSeverity.Warning)
                .AddItem("enabled", "Enabled", isActiveAndEnabled.ToString());
        }
    }
}
