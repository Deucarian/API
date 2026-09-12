using System;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Core;
using Deucarian.API.Models;
using UnityEngine;
namespace Deucarian.API.Samples.DefinitionWorkflow
{
    [DefaultExecutionOrder(-2000)]
    public sealed class SampleApiSetup : MonoBehaviour
    {
        [SerializeField] private ApiHost host;
        private void Awake() => host.Configure(new[] { ApiEndpointBinding.For(SampleEndpoints.Profile, new LocalClient(), value => new ApiRequest("sample/profile")) });
        // The sample substitutes only transport. Host binding, types and cancellation are the production path.
        private sealed class LocalClient : IApiClient
        {
            public Task<ApiResult<T>> SendAsync<T>(ApiRequest request, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (typeof(T) != typeof(string)) throw new InvalidOperationException("The sample profile returns a string.");
                return Task.FromResult(ApiResult<T>.Success((T)(object)"Hello from the local profile endpoint.", HttpMethod.GET, 200, "sample/profile", string.Empty));
            }
            public Task<ApiResult<T>> SendAsync<T>(ApiEndpoint endpoint, CancellationToken cancellationToken = default) => SendAsync<T>(endpoint.CreateRequest(), cancellationToken);
            public Task<ApiResult<T>> SendAsync<T>(ApiEndpoint endpoint, object body, CancellationToken cancellationToken = default) => SendAsync<T>(endpoint.CreateRequest(body), cancellationToken);
            public Task<ApiResult<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default) => SendAsync<T>(new ApiRequest(endpoint), cancellationToken);
            public Task<ApiResult<T>> PostAsync<T>(string endpoint, object body, CancellationToken cancellationToken = default) => SendAsync<T>(new ApiRequest(endpoint), cancellationToken);
            public Task<ApiResult<T>> PutAsync<T>(string endpoint, object body, CancellationToken cancellationToken = default) => SendAsync<T>(new ApiRequest(endpoint), cancellationToken);
            public Task<ApiResult<T>> PatchAsync<T>(string endpoint, object body, CancellationToken cancellationToken = default) => SendAsync<T>(new ApiRequest(endpoint), cancellationToken);
            public Task<ApiResult<T>> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default) => SendAsync<T>(new ApiRequest(endpoint), cancellationToken);
        }
    }
}
