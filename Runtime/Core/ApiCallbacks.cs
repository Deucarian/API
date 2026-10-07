using System;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Models;
using UnityEngine;

namespace Deucarian.API.Core
{
    /// <summary>Runs an API operation and delivers one typed completion on Unity's main thread.</summary>
    /// <remarks>
    /// Unity initializes this adapter automatically in the Editor and player. Both callbacks are required.
    /// Request construction also runs on the main thread. Completion is always posted, including cached results,
    /// so callbacks never run inside the initiating call. Cancellation is reported through onFailure.
    /// Callback exceptions are contained and never trigger the other callback. Calls made before Unity has
    /// initialized its synchronization context throw an actionable exception without starting the operation.
    /// </remarks>
    public static class ApiCallbacks
    {
        #region Fields

        private static SynchronizationContext _mainContext;
        private static int _mainThreadId;

        #endregion

        #region Methods

        /// <summary>
        /// Starts an operation with separate success and failure callbacks; existing errors are preserved.
        /// </summary>
        public static void Run<T>(Func<Task<ApiResult<T>>> operation,
                                  Action<T> onSuccess,
                                  Action<ApiError> onFailure,
                                  CancellationToken cancellationToken = default)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
            if (onSuccess == null)
                throw new ArgumentNullException(nameof(onSuccess));
            if (onFailure == null)
                throw new ArgumentNullException(nameof(onFailure));

            SynchronizationContext context = Volatile.Read(ref _mainContext);
            if (context == null)
            {
                throw new InvalidOperationException(
                    "Unity has not initialized API callbacks. Start requests from Awake/Start or an Editor action "
                    + "after Editor initialization. Outside Unity, use the Task-based API."
                );
            }

            if (Thread.CurrentThread.ManagedThreadId == _mainThreadId)
            {
                _ = ExecuteAsync(operation, onSuccess, onFailure, cancellationToken, context);
            }
            else
            {
                context.Post(state =>
                {
                    _ = ExecuteAsync(operation, onSuccess, onFailure, cancellationToken, context);
                }, null
                );
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetContext()
        {
            Volatile.Write(ref _mainContext, null);
            _mainThreadId = 0;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void InitializeMainThread()
        {
            SynchronizationContext context = SynchronizationContext.Current;
            if (context == null || context.GetType() == typeof(SynchronizationContext))
                return;

            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            Volatile.Write(ref _mainContext, context);
        }

        private static async Task ExecuteAsync<T>(Func<Task<ApiResult<T>>> operation,
                                                  Action<T> onSuccess,
                                                  Action<ApiError> onFailure,
                                                  CancellationToken cancellationToken,
                                                  SynchronizationContext context)
        {
            ApiResult<T> result;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                Task<ApiResult<T>> request = operation();
                if (request == null)
                    throw new InvalidOperationException("The API operation did not return a task.");

                result = await AwaitResultAsync(request, cancellationToken).ConfigureAwait(false);
                if (result == null)
                    throw new InvalidOperationException("The API operation did not return a result.");
            }
            catch (OperationCanceledException exception)
            {
                result = ApiResult<T>.Failure(new ApiError
                {
                    Message = "API request was canceled.",
                    IsCancellation = true,
                    Exception = exception
                }, HttpMethod.GET
                );
            }
            catch (Exception exception)
            {
                result = ApiResult<T>.Failure(new ApiError
                {
                    Message = "The API request could not be completed.",
                    Exception = exception
                }, HttpMethod.GET
                );
            }

            context.Post(_ => Complete(result, onSuccess, onFailure), null);
        }

        private static async Task<ApiResult<T>> AwaitResultAsync<T>(Task<ApiResult<T>> request,
                                                                   CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
                return await request.ConfigureAwait(false);

            TaskCompletionSource<bool> canceled = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(() => canceled.TrySetResult(true)))
            {
                Task completed = await Task.WhenAny(request, canceled.Task).ConfigureAwait(false);
                if (completed == request)
                    return await request.ConfigureAwait(false);

                // Observe a late failure without sending another completion or logging request data.
                _ = ObserveAsync(request);
                cancellationToken.ThrowIfCancellationRequested();
                throw new OperationCanceledException(cancellationToken);
            }
        }

        private static async Task ObserveAsync<T>(Task<ApiResult<T>> request)
        {
            try
            {
                await request.ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Cancellation already completed this call. Its underlying task is now observed.
            }
        }

        private static void Complete<T>(ApiResult<T> result, Action<T> onSuccess, Action<ApiError> onFailure)
        {
            try
            {
                if (result.IsSuccess)
                {
                    onSuccess(result.Data);
                }
                else
                {
                    onFailure(result.Error ?? new ApiError
                    {
                        Message = "The API request failed without details.",
                        HttpStatusCode = result.HttpStatusCode,
                        RequestUrl = result.RequestUrl,
                        RawResponseBody = result.RawResponseBody
                    });
                }
            }
            catch (Exception)
            {
                ApiLog.General.Warning("An API completion callback threw. Handle callback exceptions in the caller.");
            }
        }

        #endregion
    }
}
