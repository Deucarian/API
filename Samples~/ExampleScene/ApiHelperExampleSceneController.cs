using System.Threading;
using System.Threading.Tasks;
using JorisHoef.APIHelper.Configuration;
using JorisHoef.APIHelper.Core;
using JorisHoef.APIHelper.Models;
using UnityEngine;

namespace JorisHoef.APIHelper.Samples
{
    /// <summary>
    /// Example scene controller that demonstrates ApiClientConfig, IApiClient creation,
    /// GET/POST calls, and ApiResult handling.
    /// </summary>
    public sealed class ApiHelperExampleSceneController : MonoBehaviour
    {
        [Header("APIHelper")]
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
                Debug.LogWarning("[APIHelper Sample] No ApiClientConfig assigned. Using a runtime default config.");
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
                Debug.LogError("[APIHelper Sample] API client was not created.");
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
                Debug.LogError("[APIHelper Sample] " + label + " endpoint definition is not assigned.");
                return false;
            }

            if (!endpointDefinition.IsValid(out string message))
            {
                Debug.LogError("[APIHelper Sample] " + label + " endpoint definition is invalid: " + message);
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
                    title = "APIHelper sample",
                    body = "Created from the APIHelper example scene.",
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
                Debug.Log("[APIHelper Sample] " + label + " succeeded. Raw response: " + result.RawResponseBody);
                return;
            }

            ApiError error = result.Error;
            Debug.LogError("[APIHelper Sample] " + label + " failed: " + error?.Message);
            Debug.LogError("[APIHelper Sample] Status=" + error?.HttpStatusCode + " Url=" + error?.RequestUrl);
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
