using System;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Models;

namespace Deucarian.API.Core
{
    /// <summary>Explicit request mapping for one typed endpoint; clients and serialization remain externally composed.</summary>
    public abstract class ApiEndpointBinding
    {
        private ApiEndpointBinding(string id) { Id = id; }
        public string Id { get; }

        public static ApiEndpointBinding For<TRequest, TResponse>(EndpointKey<TRequest, TResponse> key,
            IApiClient client, Func<TRequest, ApiRequest> createRequest) =>
            new Typed<TRequest, TResponse>(key != null ? key.Id : throw new ArgumentNullException(nameof(key)), client, createRequest);

        internal sealed class Typed<TRequest, TResponse> : ApiEndpointBinding
        {
            private readonly IApiClient client;
            private readonly Func<TRequest, ApiRequest> createRequest;
            public Typed(string id, IApiClient client, Func<TRequest, ApiRequest> createRequest) : base(id)
            {
                this.client = client ?? throw new ArgumentNullException(nameof(client));
                this.createRequest = createRequest ?? throw new ArgumentNullException(nameof(createRequest));
            }

            public Task<ApiResult<TResponse>> SendAsync(TRequest value, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var request = createRequest(value) ?? throw new InvalidOperationException("The request mapping for endpoint '" + Id + "' returned null. Return an ApiRequest from this endpoint's binding.");
                return client.SendAsync<TResponse>(request, cancellationToken);
            }
        }
    }
}
