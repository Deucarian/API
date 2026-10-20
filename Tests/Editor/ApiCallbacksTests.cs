using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Core;
using Deucarian.API.Models;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;

namespace Deucarian.API.Tests
{
    public sealed class ApiCallbacksTests
    {
        #region Test Methods

        [UnityTest]
        public IEnumerator CachedSuccessIsDeferredAndDeliveredOnce()
        {
            int successes = 0;
            int failures = 0;
            int value = 0;
            ApiCallbacks.Run(() => Task.FromResult(Success(42)),
                             data => { successes++; value = data; },
                             error => failures++
            );
            Assert.That(successes, Is.Zero, "Even completed tasks must not invoke callbacks reentrantly.");
            yield return WaitFor(() => successes + failures > 0);
            Assert.That(successes, Is.EqualTo(1));
            Assert.That(failures, Is.Zero);
            Assert.That(value, Is.EqualTo(42));
        }

        [UnityTest]
        public IEnumerator HttpFailurePreservesTheOriginalErrorAndMetadata()
        {
            ApiError original = new ApiError
            {
                Message = "Validation failed.",
                HttpStatusCode = 422,
                BackendCode = "INVALID_INPUT",
                IsTimeout = true
            };
            original.ValidationErrors.Add("name", new[] { "Required" });
            original.ResponseHeaders.Add("X-Test", "retained");
            ApiError received = null;
            int successes = 0;
            ApiCallbacks.Run(() => Task.FromResult(ApiResult<int>.Failure(original, HttpMethod.POST)),
                             value => successes++,
                             error => received = error
            );
            yield return WaitFor(() => received != null);
            Assert.That(successes, Is.Zero);
            Assert.That(received, Is.SameAs(original));
            Assert.That(received.ValidationErrors["name"][0], Is.EqualTo("Required"));
        }

        [UnityTest]
        public IEnumerator SynchronousFactoryErrorUsesFailure()
        {
            InvalidOperationException original = new InvalidOperationException("Private request details");
            ApiError received = null;
            int successes = 0;
            ApiCallbacks.Run<int>(() => throw original, value => successes++, error => received = error);
            yield return WaitFor(() => received != null);
            Assert.That(successes, Is.Zero);
            Assert.That(received.Exception, Is.SameAs(original));
            Assert.That(received.Message, Does.Not.Contain("Private request details"));
        }

        [UnityTest]
        public IEnumerator FaultedTaskUsesFailure()
        {
            Exception original = new Exception("Private response details");
            ApiError received = null;
            ApiCallbacks.Run(() => Task.FromException<ApiResult<int>>(original),
                             value => { },
                             error => received = error
            );
            yield return WaitFor(() => received != null);
            Assert.That(received.Exception, Is.SameAs(original));
            Assert.That(received.IsCancellation, Is.False);
        }

        [UnityTest]
        public IEnumerator PrecancelledRequestDoesNotRunFactory()
        {
            bool started = false;
            ApiError received = null;
            ApiCallbacks.Run(() => { started = true; return Task.FromResult(Success(1)); },
                             value => { },
                             error => received = error,
                             new CancellationToken(true)
            );
            yield return WaitFor(() => received != null);
            Assert.That(started, Is.False);
            Assert.That(received.IsCancellation, Is.True);
        }

        [UnityTest]
        public IEnumerator CancellationExceptionUsesTypedFailure()
        {
            ApiError received = null;
            ApiCallbacks.Run<int>(() => throw new OperationCanceledException(),
                                  value => { },
                                  error => received = error
            );
            yield return WaitFor(() => received != null);
            Assert.That(received.IsCancellation, Is.True);
        }

        [UnityTest]
        public IEnumerator CancellationWinsOnceEvenWhenUnderlyingTaskFinishesLate()
        {
            TaskCompletionSource<ApiResult<int>> pending = new TaskCompletionSource<ApiResult<int>>();
            using (CancellationTokenSource cancellation = new CancellationTokenSource())
            {
                int completions = 0;
                ApiError received = null;
                ApiCallbacks.Run(() => pending.Task,
                                 value => completions++,
                                 error => { completions++; received = error; },
                                 cancellation.Token
                );
                cancellation.Cancel();
                yield return WaitFor(() => received != null);
                pending.SetException(new Exception("Late private payload"));
                for (int frame = 0; frame < 5; frame++)
                {
                    yield return null;
                }

                Assert.That(received.IsCancellation, Is.True);
                Assert.That(completions, Is.EqualTo(1));
            }
        }

        [UnityTest]
        public IEnumerator ThrowingSuccessCallbackDoesNotBecomeAFailure()
        {
            int successes = 0;
            int failures = 0;
            LogAssert.Expect(LogType.Warning,
                             "[Api] An API completion callback threw. Handle callback exceptions in the caller."
            );
            ApiCallbacks.Run(() => Task.FromResult(Success(1)),
                             value => { successes++; throw new Exception("Private callback data"); },
                             error => failures++
            );
            yield return WaitFor(() => successes > 0);
            Assert.That(successes, Is.EqualTo(1));
            Assert.That(failures, Is.Zero);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator ThrowingFailureCallbackIsNotInvokedTwice()
        {
            int completions = 0;
            LogAssert.Expect(LogType.Warning,
                             "[Api] An API completion callback threw. Handle callback exceptions in the caller."
            );
            ApiCallbacks.Run<int>(() => throw new Exception(),
                                  value => completions++,
                                  error => { completions++; throw new Exception("Private callback data"); }
            );
            yield return WaitFor(() => completions > 0);
            Assert.That(completions, Is.EqualTo(1));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator WorkerInvocationStartsOperationAndCompletesOnUnityThread()
        {
            int unityThread = Thread.CurrentThread.ManagedThreadId;
            int factoryThread = 0;
            int callbackThread = 0;
            ApiError received = null;
            Task worker = Task.Run(() => ApiCallbacks.Run(() =>
                                                         {
                                                             factoryThread = Thread.CurrentThread.ManagedThreadId;
                                                             return Task.Run(() => Success(1));
                                                         },
                                                         value => callbackThread = Thread.CurrentThread.ManagedThreadId,
                                                         error => received = error
            ));
            yield return WaitFor(() => callbackThread != 0 || received != null || worker.IsFaulted);
            Assert.That(worker.IsFaulted, Is.False, worker.Exception?.ToString());
            Assert.That(received, Is.Null);
            Assert.That(factoryThread, Is.EqualTo(unityThread));
            Assert.That(callbackThread, Is.EqualTo(unityThread));
        }

        [UnityTest]
        public IEnumerator NullTaskAndNullResultBothHaveFailureCallbacks()
        {
            int failures = 0;
            ApiCallbacks.Run<int>(() => null, value => { }, error => failures++);
            ApiCallbacks.Run(() => Task.FromResult<ApiResult<int>>(null), value => { }, error => failures++);
            yield return WaitFor(() => failures == 2);
            Assert.That(failures, Is.EqualTo(2));
        }

        [Test]
        public void BothCallbacksAreRequiredBeforeAnOperationCanStart()
        {
            bool started = false;
            Func<Task<ApiResult<int>>> operation = () => { started = true; return Task.FromResult(Success(1)); };
            Assert.Throws<ArgumentNullException>(() => ApiCallbacks.Run(operation, null, error => { }));
            Assert.Throws<ArgumentNullException>(() => ApiCallbacks.Run(operation, value => { }, null));
            Assert.That(started, Is.False);
        }

        [Test]
        public void MissingUnityContextRejectsRequestWithActionableGuidance()
        {
            SynchronizationContext previous = SynchronizationContext.Current;
            MethodInfo reset = typeof(ApiCallbacks).GetMethod("ResetContext",
                                                              BindingFlags.Static | BindingFlags.NonPublic
            );
            try
            {
                reset.Invoke(null, null);
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                ApiCallbacks.InitializeMainThread();
                bool started = false;
                InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                    ApiCallbacks.Run(() => { started = true; return Task.FromResult(Success(1)); },
                                     value => { },
                                     error => { }
                    )
                );
                Assert.That(started, Is.False);
                Assert.That(exception.Message, Does.Contain("Awake/Start"));
                Assert.That(exception.Message, Does.Contain("Task-based API"));
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
                ApiCallbacks.InitializeMainThread();
            }
        }

        #endregion

        #region Methods

        private static ApiResult<int> Success(int value)
            => ApiResult<int>.Success(value, HttpMethod.GET, 200, null, null);

        private static IEnumerator WaitFor(Func<bool> condition)
        {
            Stopwatch deadline = Stopwatch.StartNew();
            while (!condition() && deadline.Elapsed < TimeSpan.FromSeconds(10))
            {
                yield return null;
            }

            Assert.That(condition(), Is.True, "The API completion callback was not delivered.");
        }

        #endregion
    }
}
