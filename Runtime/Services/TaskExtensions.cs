using System;
using System.Collections;
using System.Threading.Tasks;

namespace Deucarian.API.Services
{
    /// <summary>
    /// Small Unity coroutine adapters for legacy API task workflows.
    /// </summary>
    public static class TaskExtensions
    {
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
