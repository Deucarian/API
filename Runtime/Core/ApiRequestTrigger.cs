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
        /// <summary>Sends this component's request using typed success and failure callbacks.</summary>
        public void Send(TRequest value,
                         Action<TResponse> onSuccess,
                         Action<ApiError> onFailure,
                         CancellationToken cancellationToken = default)
        {
            ApiCallbacks.Run(() => SendAsync(value, cancellationToken),
                             onSuccess,
                             onFailure,
                             cancellationToken
            );
        }

        /// <summary>Sends the configured request and raises the serialized completion event.</summary>
        public void Send()
        {
            ApiResult<TResponse> completedResult = null;
            ApiCallbacks.Run(async () =>
                             {
                                 completedResult = await SendAsync(request, lifetime.Token);
                                 return completedResult;
                             },
                             value =>
                             {
                                 if (this == null)
                                     return;

                                 LastResult = completedResult;
                                 succeeded.Invoke(value);
                             },
                             error =>
                             {
                                 if (this == null)
                                     return;

                                 if (completedResult != null && completedResult.IsFailure
                                     && completedResult.Error == null && !error.IsCancellation)
                                 {
                                     completedResult.Error = error;
                                 }

                                 LastResult = completedResult != null && ReferenceEquals(completedResult.Error, error)
                                     ? completedResult
                                     : ApiResult<TResponse>.Failure(error, HttpMethod.GET);
                                 failed.Invoke();
                             },
                             lifetime.Token
            );
        }

        protected virtual void OnDestroy() { lifetime.Cancel(); lifetime.Dispose(); }
    }
}
