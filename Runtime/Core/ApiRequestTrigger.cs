using System;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Models;
using UnityEngine;
using UnityEngine.Events;
namespace Deucarian.API.Core
{
    /// <summary>Derive a concrete component with your request/response types; configure the host once.</summary>
    public abstract class ApiRequestTrigger<TRequest, TResponse> : MonoBehaviour
    {
        [SerializeField] private ApiHost host;
        [SerializeField] private EndpointKey<TRequest, TResponse> endpoint;
        [SerializeField] private TRequest request;
        [SerializeField] private UnityEvent<TResponse> succeeded = new UnityEvent<TResponse>();
        [SerializeField] private UnityEvent failed = new UnityEvent();
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        public ApiResult<TResponse> LastResult { get; private set; }
        public Task<ApiResult<TResponse>> SendAsync(TRequest value, CancellationToken cancellationToken = default)
        {
            if (host == null) throw new InvalidOperationException("Assign a configured ApiHost to this request component.");
            return host.SendAsync(endpoint, value, cancellationToken);
        }
        public async void Send()
        {
            ApiResult<TResponse> result;
            try { result = await SendAsync(request, lifetime.Token); }
            catch (OperationCanceledException) { return; }
            if (this == null) return;
            LastResult = result;
            if (result.IsSuccess) succeeded.Invoke(result.Data); else failed.Invoke();
        }
        protected virtual void OnDestroy() { lifetime.Cancel(); lifetime.Dispose(); }
    }
}
