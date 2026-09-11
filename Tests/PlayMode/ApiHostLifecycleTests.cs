using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Core;
using Deucarian.API.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Deucarian.API.Tests
{
    public sealed class ApiHostLifecycleTests
    {
        [UnityTest]
        public IEnumerator DestroyCancelsInFlightRequestWithoutDisposingBorrowedClient()
        {
            var go = new GameObject("api scope");
            var client = new WaitingClient();
            try
            {
                var host = go.AddComponent<ApiHost>();
                var key = new ProfileKey();
                host.Configure(new[] { ApiEndpointBinding.For(key, client, value => new ApiRequest("profile")) });
                var task = host.SendAsync(key, "request");
                Assert.That(task.IsCompleted, Is.False);
                UnityEngine.Object.DestroyImmediate(go);
                for (int i = 0; i < 30 && !task.IsCompleted; i++) yield return null;
                Assert.That(task.IsCanceled, Is.True);
                Assert.That(client.Disposed, Is.False);
            }
            finally { if (go != null) UnityEngine.Object.DestroyImmediate(go); }
        }

        [Test]
        public void MissingBindingAndPrecancelledRequestHaveExplicitOutcomes()
        {
            var go = new GameObject("api scope");
            try
            {
                var host = go.AddComponent<ApiHost>();
                var key = new ProfileKey();
                Assert.That(Assert.ThrowsAsync<InvalidOperationException>(async () => await host.SendAsync(key, "request")).Message,
                    Does.Contain("Configure"));
                bool mapped = false;
                host.Configure(new[] { ApiEndpointBinding.For(key, new WaitingClient(), value => { mapped = true; return new ApiRequest("profile"); }) });
                Assert.ThrowsAsync(Is.InstanceOf<OperationCanceledException>(), async () => await host.SendAsync(key, "request", new CancellationToken(true)));
                Assert.That(mapped, Is.False);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        private sealed class ProfileKey : EndpointKey<string, int> { public ProfileKey() : base("profile") { } }
        private sealed class WaitingClient : IApiClient, IDisposable
        {
            public bool Disposed { get; private set; }
            public void Dispose() => Disposed = true;
            public async Task<ApiResult<T>> SendAsync<T>(ApiRequest request, CancellationToken cancellationToken = default)
            { await Task.Delay(Timeout.Infinite, cancellationToken); return null; }
            public Task<ApiResult<T>> SendAsync<T>(ApiEndpoint endpoint, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<ApiResult<T>> SendAsync<T>(ApiEndpoint endpoint, object body, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<ApiResult<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<ApiResult<T>> PostAsync<T>(string endpoint, object body, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<ApiResult<T>> PutAsync<T>(string endpoint, object body, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<ApiResult<T>> PatchAsync<T>(string endpoint, object body, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<ApiResult<T>> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        }
    }
}
