using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Configuration;
using Deucarian.API.Core;
using Deucarian.API.Models;
using UnityEngine;

namespace Deucarian.API.Samples
{
    /// <summary>
    /// Example scene controller that demonstrates ApiClientConfig, IApiClient creation,
    /// GET/POST calls, and ApiResult handling.
    /// </summary>
    public sealed class ApiExampleSceneController : MonoBehaviour
    {
        [Header("API")]
        [SerializeField] private ApiClientConfig apiClientConfig;
        [SerializeField] private ApiEndpointDefinition getPostEndpoint;
        [SerializeField] private ApiEndpointDefinition createPostEndpoint;

        [Header("Example")]
        [Tooltip("When enabled, runs the sample GET and POST requests on Start.")]
        [SerializeField] private bool runOnStart;

        private IApiClient apiClient;
        private CancellationTokenSource cancellationTokenSource;

        private void Awake()
        {
            cancellationTokenSource = new CancellationTokenSource();

            if (apiClientConfig == null)
            {
                ApiLog.Samples.Warning("No ApiClientConfig assigned. Using a runtime default config.");
            }

            apiClient = ApiClientFactory.Create(apiClientConfig);
        }

        private async void Start()
        {
            if (runOnStart)
            {
                await RunExampleRequestsAsync();
            }
        }

        private void OnDestroy()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }

        [ContextMenu("Run Example Requests")]
        private async void RunExampleRequestsFromContextMenu()
        {
            await RunExampleRequestsAsync();
        }

        /// <summary>
        /// Runs the sample GET and POST calls using the configured endpoint assets.
        /// </summary>
        /// <returns>A task that completes after both sample requests have finished.</returns>
        public async Task RunExampleRequestsAsync()
        {
            if (apiClient == null)
            {
                ApiLog.Samples.Error("API client was not created.");
                return;
            }

            if (!ValidateEndpoint("GET", getPostEndpoint) ||
                !ValidateEndpoint("POST", createPostEndpoint))
            {
                return;
            }

            await RunGetExampleAsync(cancellationTokenSource.Token);
            await RunPostExampleAsync(cancellationTokenSource.Token);
        }

        private static bool ValidateEndpoint(string label, ApiEndpointDefinition endpointDefinition)
        {
            if (endpointDefinition == null)
            {
                ApiLog.Samples.Error(label + " endpoint definition is not assigned.");
                return false;
            }

            if (!endpointDefinition.IsValid(out string message))
            {
                ApiLog.Samples.Error(label + " endpoint definition is invalid: " + message);
                return false;
            }

            return true;
        }

        private async Task RunGetExampleAsync(CancellationToken cancellationToken)
        {
            ApiResult<ExamplePostDto> result =
                    await apiClient.SendAsync<ExamplePostDto>(getPostEndpoint, cancellationToken);

            LogResult("GET", result);
        }

        private async Task RunPostExampleAsync(CancellationToken cancellationToken)
        {
            ExampleCreatePostRequest body = new ExampleCreatePostRequest
            {
                    title = "API sample",
                    body = "Created from the API example scene.",
                    userId = 1
            };

            ApiResult<ExamplePostDto> result =
                    await apiClient.SendAsync<ExamplePostDto>(createPostEndpoint, body, cancellationToken);

            LogResult("POST", result);
        }

        private static void LogResult<T>(string label, ApiResult<T> result)
        {
            if (result.IsSuccess)
            {
                ApiLog.Samples.Info(label + " succeeded. Raw response: " + result.RawResponseBody);
                return;
            }

            ApiError error = result.Error;
            ApiLog.Samples.Error(label + " failed: " + error?.Message);
            ApiLog.Samples.Error("Status=" + error?.HttpStatusCode + " Url=" + error?.RequestUrl);
        }

        [System.Serializable]
        private sealed class ExampleCreatePostRequest
        {
            public string title;
            public string body;
            public int userId;
        }

        [System.Serializable]
        private sealed class ExamplePostDto
        {
            public int userId;
            public int id;
            public string title;
            public string body;
        }
    }
}
