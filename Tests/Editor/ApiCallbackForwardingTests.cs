using System;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Configuration;
using Deucarian.API.Core;
using Deucarian.API.Models;
using Deucarian.API.Services;
using Deucarian.API.Services.Base;
using Deucarian.API.Services.MultipartForm;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;

namespace Deucarian.API.Tests
{
    public sealed class ApiCallbackForwardingTests
    {
        #region Test Methods

        [UnityTest]
        public IEnumerator ClientCallbackShapesForwardTheirOperationArguments()
        {
            RecordingClient client = new RecordingClient();
            ApiRequest request = new ApiRequest("advanced", HttpMethod.PATCH);
            ApiEndpoint endpoint = new ApiEndpoint("typed", HttpMethod.PUT);
            object body = new object();
            ApiEndpointDefinition definition = ScriptableObject.CreateInstance<ApiEndpointDefinition>();
            definition.Path = "asset";
            definition.Method = HttpMethod.POST;
            try
            {
                using (CancellationTokenSource cancellation = new CancellationTokenSource())
                {
                    int successes = 0;
                    ApiError failure = null;
                    Action<int> onSuccess = value => successes++;
                    Action<ApiError> onFailure = error => failure = error;
                    CancellationToken token = cancellation.Token;
                    client.Send(request, onSuccess, onFailure, token);
                    Assert.That(client.LastRequest, Is.SameAs(request));
                    client.Send(endpoint, onSuccess, onFailure, token);
                    Assert.That(client.LastEndpoint, Is.SameAs(endpoint));
                    client.Send(endpoint, body, onSuccess, onFailure, token);
                    Assert.That(client.LastBody, Is.SameAs(body));
                    client.Send(definition, onSuccess, onFailure, token);
                    Assert.That(client.LastEndpoint.Path, Is.EqualTo("asset"));
                    client.Send(definition, body, onSuccess, onFailure, token);
                    Assert.That(client.LastBody, Is.SameAs(body));
                    client.Get("get", onSuccess, onFailure, token);
                    Assert.That(client.LastVerb, Is.EqualTo(HttpMethod.GET));
                    client.Post("post", body, onSuccess, onFailure, token);
                    Assert.That(client.LastVerb, Is.EqualTo(HttpMethod.POST));
                    client.Put("put", body, onSuccess, onFailure, token);
                    Assert.That(client.LastVerb, Is.EqualTo(HttpMethod.PUT));
                    client.Patch("patch", body, onSuccess, onFailure, token);
                    Assert.That(client.LastVerb, Is.EqualTo(HttpMethod.PATCH));
                    client.Delete("delete", onSuccess, onFailure, token);
                    Assert.That(client.LastVerb, Is.EqualTo(HttpMethod.DELETE));
                    Assert.That(client.LastToken, Is.EqualTo(token));
                    yield return WaitFor(() => successes == 10 || failure != null);
                    Assert.That(failure, Is.Null);
                    Assert.That(successes, Is.EqualTo(10));
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }

        [UnityTest]
        public IEnumerator InvalidEndpointDefinitionUsesFailureInsteadOfThrowing()
        {
            RecordingClient client = new RecordingClient();
            ApiError failure = null;
            client.Send<int>((ApiEndpointDefinition)null, value => { }, error => failure = error);
            yield return WaitFor(() => failure != null);
            Assert.That(failure.Exception, Is.TypeOf<ArgumentNullException>());
            Assert.That(client.LastEndpoint, Is.Null);
        }

        [UnityTest]
        public IEnumerator LegacyMultipartAndFacadeRetainErrorDetailsAndForwardCancellation()
        {
            IApiClient previous = ApiServices.Client;
            RecordingClient client = new RecordingClient();
            ApiError original = new ApiError { HttpStatusCode = 409, BackendCode = "CONFLICT" };
            client.Responses.Enqueue(ApiResult<int>.Failure(original, HttpMethod.POST));
            ApiServices.Configure(client);
            try
            {
                using (CancellationTokenSource cancellation = new CancellationTokenSource())
                {
                    object body = new object();
                    ApiError failure = null;
#pragma warning disable CS0618
                    ApiServices.PostMultipart<int>("upload",
                                                   body,
                                                   true,
                                                   value => { },
                                                   error => failure = error,
                                                   accessToken: "test-token",
                                                   cancellationToken: cancellation.Token
                    );
#pragma warning restore CS0618
                    yield return WaitFor(() => failure != null);
                    Assert.That(failure, Is.SameAs(original));
                    Assert.That(client.LastRequest.BodyFormat, Is.EqualTo(ApiRequestBodyFormat.MultipartForm));
                    Assert.That(client.LastRequest.Body, Is.SameAs(body));
                    Assert.That(client.LastToken, Is.EqualTo(cancellation.Token));

                    int received = 0;
                    ApiServices.Get<int>("current", value => received = value, error => failure = error);
                    yield return WaitFor(() => received == 42);
                    Assert.That(received, Is.EqualTo(42));
                }
            }
            finally
            {
                ApiServices.Configure(previous);
            }
        }

        [UnityTest]
        public IEnumerator InternalLegacyServiceCallbacksKeepFailureMetadataAndSanitizeLogs()
        {
            IApiClient previous = ApiServices.Client;
            RecordingClient client = new RecordingClient();
            ApiServices.Configure(client);
            try
            {
                ApiService<int>[] services =
                {
                    new ApiService<int>(HttpMethod.POST),
                    new MultipartApiService<int>(HttpMethod.POST)
                };
                foreach (ApiService<int> service in services)
                {
                    ApiError original = new ApiError
                    {
                        Message = "Failure",
                        HttpStatusCode = 503,
                        IsCancellation = true,
                        IsTimeout = true,
                        RawResponseBody = "private-payload",
                        RequestUrl = "private-route",
                        BackendCode = "RETRY"
                    };
                    original.ResponseHeaders.Add("X-Test", "retained");
                    client.Responses.Enqueue(ApiResult<int>.Failure(original, HttpMethod.POST));
                    ApiError received = null;
                    LogAssert.Expect(LogType.Error,
                                     "[Api.Requests] A legacy API request failed. Inspect its ApiError for details."
                    );
                    service.Execute("private-route", false, value => { }, error => received = error);
                    yield return WaitFor(() => received != null);
                    Assert.That(received, Is.SameAs(original));
                    Assert.That(received.IsCancellation, Is.True);
                    Assert.That(received.IsTimeout, Is.True);
                    Assert.That(received.HttpStatusCode, Is.EqualTo(503));
                    Assert.That(received.ResponseHeaders["X-Test"], Is.EqualTo("retained"));
                    LogAssert.NoUnexpectedReceived();
                }
            }
            finally
            {
                ApiServices.Configure(previous);
            }
        }

        [UnityTest]
        public IEnumerator TriggerEventsSeeTheirOwnCompleteResults()
        {
            GameObject owner = new GameObject("callback trigger test");
            try
            {
                ApiHost host = owner.AddComponent<ApiHost>();
                CallbackTrigger trigger = owner.AddComponent<CallbackTrigger>();
                RecordingClient client = new RecordingClient();
                ProfileKey key = new ProfileKey();
                host.Configure(new[] { ApiEndpointBinding.For(key, client, value => new ApiRequest("profile")) });
                SetTriggerField(trigger, "host", host);
                SetTriggerField(trigger, "endpoint", key);
                ApiResult<int> first = ApiResult<int>.Success(1, HttpMethod.POST, 201, "first", null);
                ApiResult<int> second = ApiResult<int>.Failure(new ApiError { HttpStatusCode = 409 }, HttpMethod.PUT);
                second.RequestUrl = "result-only-metadata";
                client.Responses.Enqueue(first);
                client.Responses.Enqueue(second);
                List<ApiResult<int>> seen = new List<ApiResult<int>>();
                GetTriggerField<UnityEvent<int>>(trigger, "succeeded")
                    .AddListener(value => seen.Add(trigger.LastResult));
                GetTriggerField<UnityEvent>(trigger, "failed").AddListener(() => seen.Add(trigger.LastResult));
                trigger.Send();
                trigger.Send();
                Assert.That(trigger.LastResult, Is.Null, "LastResult belongs to the delivered event.");
                yield return WaitFor(() => seen.Count == 2);
                Assert.That(seen[0], Is.SameAs(first));
                Assert.That(seen[1], Is.SameAs(second));
                Assert.That(seen[1].HttpMethod, Is.EqualTo(HttpMethod.PUT));
                Assert.That(seen[1].RequestUrl, Is.EqualTo("result-only-metadata"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(owner);
            }
        }

        [UnityTest]
        public IEnumerator TriggerPreservesFailedResultMetadataWhenErrorDetailsAreMissing()
        {
            GameObject owner = new GameObject("callback trigger fallback test");
            try
            {
                ApiHost host = owner.AddComponent<ApiHost>();
                CallbackTrigger trigger = owner.AddComponent<CallbackTrigger>();
                RecordingClient client = new RecordingClient();
                ProfileKey key = new ProfileKey();
                host.Configure(new[] { ApiEndpointBinding.For(key, client, value => new ApiRequest("profile")) });
                SetTriggerField(trigger, "host", host);
                SetTriggerField(trigger, "endpoint", key);
                ApiResult<int> response = new ApiResult<int>
                {
                    IsSuccess = false,
                    HttpMethod = HttpMethod.POST,
                    HttpStatusCode = 503,
                    RequestUrl = "result-only-route",
                    RawResponseBody = "result-only-response",
                    Data = 7
                };
                client.Responses.Enqueue(response);
                ApiResult<int> seen = null;
                GetTriggerField<UnityEvent>(trigger, "failed").AddListener(() => seen = trigger.LastResult);
                trigger.Send();
                yield return WaitFor(() => seen != null);
                Assert.That(seen, Is.SameAs(response));
                Assert.That(seen.HttpMethod, Is.EqualTo(HttpMethod.POST));
                Assert.That(seen.HttpStatusCode, Is.EqualTo(503));
                Assert.That(seen.RequestUrl, Is.EqualTo("result-only-route"));
                Assert.That(seen.RawResponseBody, Is.EqualTo("result-only-response"));
                Assert.That(seen.Data, Is.EqualTo(7));
                Assert.That(seen.Error, Is.Not.Null);
                Assert.That(seen.Error.Message, Is.EqualTo("The API request failed without details."));
                Assert.That(seen.Error.HttpStatusCode, Is.EqualTo(503));
                Assert.That(seen.Error.RequestUrl, Is.EqualTo("result-only-route"));
                Assert.That(seen.Error.IsCancellation, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(owner);
            }
        }

        [UnityTest]
        public IEnumerator TriggerCancellationIsNotReplacedByLateFailureWithMissingError()
        {
            GameObject owner = new GameObject("callback trigger cancellation test");
            try
            {
                ApiHost host = owner.AddComponent<ApiHost>();
                CallbackTrigger trigger = owner.AddComponent<CallbackTrigger>();
                TaskCompletionSource<ApiResult<int>> pending = new TaskCompletionSource<ApiResult<int>>();
                RecordingClient client = new RecordingClient { PendingResponse = pending.Task };
                ProfileKey key = new ProfileKey();
                host.Configure(new[] { ApiEndpointBinding.For(key, client, value => new ApiRequest("profile")) });
                SetTriggerField(trigger, "host", host);
                SetTriggerField(trigger, "endpoint", key);
                ApiResult<int> lateResponse = new ApiResult<int>
                {
                    IsSuccess = false,
                    HttpMethod = HttpMethod.POST,
                    HttpStatusCode = 503,
                    RequestUrl = "late-response-route"
                };
                List<ApiResult<int>> seen = new List<ApiResult<int>>();
                GetTriggerField<UnityEvent>(trigger, "failed").AddListener(() => seen.Add(trigger.LastResult));
                trigger.Send();
                GetTriggerField<CancellationTokenSource>(trigger, "lifetime").Cancel();
                pending.SetResult(lateResponse);
                yield return WaitFor(() => seen.Count > 0);
                yield return null;
                Assert.That(seen.Count, Is.EqualTo(1));
                Assert.That(seen[0], Is.Not.SameAs(lateResponse));
                Assert.That(seen[0].Error.IsCancellation, Is.True);
                Assert.That(seen[0].RequestUrl, Is.Null);
                Assert.That(lateResponse.Error, Is.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(owner);
            }
        }

        #endregion

        #region Methods

        private static IEnumerator WaitFor(Func<bool> condition)
        {
            Stopwatch deadline = Stopwatch.StartNew();
            while (!condition() && deadline.Elapsed < TimeSpan.FromSeconds(10))
            {
                yield return null;
            }

            Assert.That(condition(), Is.True, "The callback was not delivered.");
        }

        private static void SetTriggerField(CallbackTrigger trigger, string name, object value)
        {
            typeof(ApiRequestTrigger<string, int>).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(trigger, value);
        }

        private static T GetTriggerField<T>(CallbackTrigger trigger, string name)
        {
            return (T)typeof(ApiRequestTrigger<string, int>)
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(trigger);
        }

        #endregion

        #region Nested Types

        public sealed class CallbackTrigger : ApiRequestTrigger<string, int> { }

        private sealed class ProfileKey : EndpointKey<string, int>
        {
            public ProfileKey() : base("test.profile") { }
        }

        private sealed class RecordingClient : IApiClient
        {
            public Queue<ApiResult<int>> Responses { get; } = new Queue<ApiResult<int>>();
            public Task<ApiResult<int>> PendingResponse { get; set; }
            public ApiRequest LastRequest { get; private set; }
            public ApiEndpoint LastEndpoint { get; private set; }
            public object LastBody { get; private set; }
            public HttpMethod LastVerb { get; private set; }
            public CancellationToken LastToken { get; private set; }

            public Task<ApiResult<T>> SendAsync<T>(ApiRequest request, CancellationToken cancellationToken = default)
            {
                LastRequest = request;
                return Respond<T>(cancellationToken);
            }

            public Task<ApiResult<T>> SendAsync<T>(ApiEndpoint endpoint, CancellationToken cancellationToken = default)
            {
                LastEndpoint = endpoint;
                return Respond<T>(cancellationToken);
            }

            public Task<ApiResult<T>> SendAsync<T>(ApiEndpoint endpoint,
                                                  object body,
                                                  CancellationToken cancellationToken = default)
            {
                LastBody = body;
                return SendAsync<T>(endpoint, cancellationToken);
            }

            public Task<ApiResult<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
                => Verb<T>(HttpMethod.GET, null, cancellationToken);

            public Task<ApiResult<T>> PostAsync<T>(string endpoint,
                                                  object body,
                                                  CancellationToken cancellationToken = default)
                => Verb<T>(HttpMethod.POST, body, cancellationToken);

            public Task<ApiResult<T>> PutAsync<T>(string endpoint,
                                                 object body,
                                                 CancellationToken cancellationToken = default)
                => Verb<T>(HttpMethod.PUT, body, cancellationToken);

            public Task<ApiResult<T>> PatchAsync<T>(string endpoint,
                                                   object body,
                                                   CancellationToken cancellationToken = default)
                => Verb<T>(HttpMethod.PATCH, body, cancellationToken);

            public Task<ApiResult<T>> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default)
                => Verb<T>(HttpMethod.DELETE, null, cancellationToken);

            private Task<ApiResult<T>> Verb<T>(HttpMethod method, object body, CancellationToken cancellationToken)
            {
                LastVerb = method;
                LastBody = body;
                return Respond<T>(cancellationToken);
            }

            private Task<ApiResult<T>> Respond<T>(CancellationToken cancellationToken)
            {
                LastToken = cancellationToken;
                if (PendingResponse != null)
                    return AwaitPendingResponse<T>(PendingResponse);

                ApiResult<int> result = Responses.Count > 0
                    ? Responses.Dequeue()
                    : ApiResult<int>.Success(42, HttpMethod.GET, 200, null, null);
                return Task.FromResult((ApiResult<T>)(object)result);
            }

            private static async Task<ApiResult<T>> AwaitPendingResponse<T>(Task<ApiResult<int>> response)
            {
                return (ApiResult<T>)(object)await response;
            }
        }

        #endregion
    }
}
