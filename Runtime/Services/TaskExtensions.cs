using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Core;
using Deucarian.API.Models;

namespace Deucarian.API.Services
{
    /// <summary>
    /// Small Unity coroutine adapters for legacy API task workflows.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>Waits for a structured API task with separate typed success/failure callbacks.</summary>
        public static IEnumerator AsIEnumeratorWithCallback<T>(this Task<ApiResult<T>> task,
                                                                Action<T> onSuccess,
                                                                Action<ApiError> onFailure,
                                                                CancellationToken cancellationToken = default)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            if (onSuccess == null)
                throw new ArgumentNullException(nameof(onSuccess));
            if (onFailure == null)
                throw new ArgumentNullException(nameof(onFailure));

            bool completed = false;
            ApiCallbacks.Run(() => task,
                             value =>
                             {
                                 completed = true;
                                 onSuccess(value);
                             },
                             error =>
                             {
                                 completed = true;
                                 onFailure(error);
                             },
                             cancellationToken
            );

            while (!completed)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Converts a task into an IEnumerator and invokes a callback when it completes successfully.
        /// </summary>
        /// <typeparam name="T">Task result type.</typeparam>
        /// <param name="task">Task to wait for.</param>
        /// <param name="onCompleted">Callback invoked with the task result on success.</param>
        /// <returns>An IEnumerator suitable for StartCoroutine.</returns>
        public static IEnumerator AsIEnumeratorWithCallback<T>(this Task<T> task, Action<T> onCompleted)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsCompletedSuccessfully)
            {
                onCompleted?.Invoke(task.Result);
            }
            else if (task.IsFaulted)
            {
                ApiLog.General.Exception(task.Exception, "Task failed with exception.");
            }
        }
    }
}
