using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Core;
using Deucarian.API.Models;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Deucarian.API.Tests
{
    public sealed class ApiCallbacksPlayModeTests
    {
        #region Test Methods

        [UnityTest]
        public IEnumerator WorkerRequestAndCallbackRunOnThePlayerMainThread()
        {
            int mainThread = Thread.CurrentThread.ManagedThreadId;
            int requestThread = 0;
            int callbackThread = 0;
            ApiError failure = null;
            Task worker = Task.Run(() => ApiCallbacks.Run(() =>
                                                         {
                                                             requestThread = Thread.CurrentThread.ManagedThreadId;
                                                             return Task.Run(() => Success(7));
                                                         },
                                                         value => callbackThread = Thread.CurrentThread.ManagedThreadId,
                                                         error => failure = error
            ));
            Stopwatch deadline = Stopwatch.StartNew();
            while (callbackThread == 0 && failure == null && !worker.IsFaulted
                   && deadline.Elapsed < TimeSpan.FromSeconds(10))
            {
                yield return null;
            }

            Assert.That(worker.IsFaulted, Is.False, worker.Exception?.ToString());
            Assert.That(failure, Is.Null);
            Assert.That(requestThread, Is.EqualTo(mainThread));
            Assert.That(callbackThread, Is.EqualTo(mainThread));
        }

        [UnityTest]
        public IEnumerator PlayerCancellationHasOneTypedFailureOnTheMainThread()
        {
            int mainThread = Thread.CurrentThread.ManagedThreadId;
            int callbackThread = 0;
            int completions = 0;
            ApiError failure = null;
            TaskCompletionSource<ApiResult<int>> pending = new TaskCompletionSource<ApiResult<int>>();
            using (CancellationTokenSource cancellation = new CancellationTokenSource())
            {
                ApiCallbacks.Run(() => pending.Task,
                                 value => completions++,
                                 error =>
                                 {
                                     callbackThread = Thread.CurrentThread.ManagedThreadId;
                                     failure = error;
                                     completions++;
                                 },
                                 cancellation.Token
                );
                Task worker = Task.Run(() => cancellation.Cancel());
                Stopwatch deadline = Stopwatch.StartNew();
                while (failure == null && deadline.Elapsed < TimeSpan.FromSeconds(10))
                {
                    yield return null;
                }

                pending.SetResult(ApiResult<int>.Success(7, HttpMethod.GET, 200, null, null));
                yield return null;
                Assert.That(worker.IsFaulted, Is.False);
                Assert.That(failure, Is.Not.Null);
                Assert.That(failure.IsCancellation, Is.True);
                Assert.That(callbackThread, Is.EqualTo(mainThread));
                Assert.That(completions, Is.EqualTo(1));
            }
        }

        #endregion

        #region Methods

        private static ApiResult<int> Success(int value)
            => ApiResult<int>.Success(value, HttpMethod.GET, 200, null, null);

        #endregion
    }
}
