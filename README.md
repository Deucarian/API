# APIHelper

## Overview

APIHelper is a reusable Unity/C# API client package built around `IApiClient`.
It wraps UnityWebRequest behind a small, testable client surface and supports
developer-friendly code endpoints, designer-friendly ScriptableObject endpoints,
structured error handling, authentication providers, cancellation, timeouts, and
safe certificate defaults.

The package is intentionally practical: use raw string routes for simple calls,
`ApiEndpoint` for richer code-defined endpoints, `ApiEndpointDefinition` assets
for non-developer configuration, and `ApiRequest` only for advanced one-off
requests.

Package ID: `com.jorishoef.api-helper`

## Core Concepts

- `IApiClient` as the main injectable dependency.
- Safe TLS behavior by default. Certificate bypass is only available as an
  explicit development-only option.
- Central `ApiClientConfig` ScriptableObject for base URL, headers, timeout,
  authentication, JSON settings, certificate handling, and logging.
- Raw string endpoint workflow for route constants.
- Lightweight `ApiEndpoint` workflow for method/auth/timeout/header/query
  defaults in code.
- `ApiEndpointDefinition` ScriptableObject workflow for designers and
  non-developers.
- Advanced `ApiRequest` for custom headers, query parameters, body format, and
  timeout overrides.
- First-class response support for JSON DTOs, `string`, `byte[]`, and
  `Texture2D`.
- Raw text, raw byte, JSON, and multipart request body formats.
- Structured `ApiResult<T>` and `ApiError` responses.
- CancellationToken support.
- Legacy `ApiServices` facade for older call sites.

## Installation

APIHelper is distributed as a Unity Package Manager Git package.

Add one of these entries to your Unity project's `Packages/manifest.json`.

Stable branch:

```json
"com.jorishoef.api-helper": "https://github.com/JorisHoef/API-Helper.git#main"
```

Beta branch:

```json
"com.jorishoef.api-helper": "https://github.com/JorisHoef/API-Helper.git#develop"
```

Release channels:

- `main` is the stable channel.
- `develop` is the beta channel.
- This local repo currently has no stable release tag. When release tags are published, use the tag name as the Git ref for immutable installs.

The package expects Unity 2021.3 or newer and declares dependencies on
`com.unity.nuget.newtonsoft-json` plus Unity's built-in UnityWebRequest modules.
Unity may pin Git package commits in `Packages/packages-lock.json`; to update,
open Package Manager and update the package, remove the lock entry, or change
the Git ref in `manifest.json`.

## Package Layout

```text
API-Helper/
  package.json
  README.md
  CHANGELOG.md
  LICENSE.md
  CONTRIBUTING.md
  Runtime/
  Editor/
  Tests/Editor/
  Samples~/ExampleScene/
```

Assembly definitions use assembly-name references instead of GUID references for
readability and package portability. The runtime assembly has no editor-only
references, the editor and test assemblies are Editor-only, and samples are kept
in their own `APIHelper.Samples` assembly.

## Public API

The main runtime APIs are:

- `IApiClient`: injectable client used by application services.
- `ApiClientFactory`: creates the default client pipeline from `ApiClientConfig`.
- `ApiClientConfig`: ScriptableObject config for base URL, default headers, timeout, auth, JSON settings, certificate handling, response format, and logging.
- `IApiAuthProvider` and `ApiAuthProviderAsset`: code and ScriptableObject token providers.
- `ApiEndpoint`: code-defined endpoint with method, auth, timeout, headers, query parameters, path parameters, and response format.
- `ApiEndpointDefinition`: ScriptableObject endpoint asset that converts to `ApiEndpoint`.
- `ApiRequest`: advanced one-off request model.
- `ApiResult<T>` and `ApiError`: success/failure result model.
- `ApiServices`: legacy/convenience static facade over a configured `IApiClient`.

Most new code should depend on `IApiClient`, create it with `ApiClientFactory.Create`, and handle `ApiResult<T>` instead of catching transport exceptions for normal API failures.

## Samples

APIHelper includes lightweight learning assets under:

`Samples~/ExampleScene`

- `ExampleScene/APIHelperExample.unity`: scene with an `ApiHelperExampleSceneController`
  wired to sample config and endpoint assets.
- `ExampleScene/ExampleApiClientConfig.asset`: config using
  `https://jsonplaceholder.typicode.com`, safe certificate defaults, and verbose
  sample logging.
- `ExampleScene/ExampleGetPostEndpoint.asset`: ScriptableObject GET endpoint.
- `ExampleScene/ExampleCreatePostEndpoint.asset`: ScriptableObject POST endpoint.
- `ExampleScene/ExampleAuthProvider.cs`: sample-only auth provider showing how a
  project can return a bearer token.

To import the sample, open Unity Package Manager, select `API Helper`, expand
`Samples`, and choose `Import` for `API Helper Example Scene`. Then open
`APIHelperExample.unity`, select the `APIHelper Example` GameObject, and use the
component context menu `Run Example Requests`. The sample calls require internet
access.

## Quick Start

Create a config asset:

`Assets > Create > JorisHoef > API Helper > Client Config`

Set:

- `Base Url`: for example `https://api.example.com/api/v1`
- `Timeout Seconds`: for example `30`
- `Certificate Handling Mode`: `Default Validation`
- `Authentication Mode`: `None` for public APIs, or `Bearer Token` when using an auth provider

Create a client:

```csharp
using System.Threading;
using System.Threading.Tasks;
using JorisHoef.APIHelper.Core;
using JorisHoef.APIHelper.Configuration;
using JorisHoef.APIHelper.Models;

public sealed class ProjectService
{
    private readonly IApiClient apiClient;

    public ProjectService(ApiClientConfig config)
    {
        apiClient = ApiClientFactory.Create(config);
    }

    public Task<ApiResult<ProjectDto[]>> ListProjectsAsync(CancellationToken cancellationToken)
    {
        return apiClient.GetAsync<ProjectDto[]>("projects", cancellationToken);
    }
}
```

Handle the result:

```csharp
ApiResult<ProjectDto[]> result = await apiClient.GetAsync<ProjectDto[]>("projects", cancellationToken);

if (result.IsSuccess)
{
    Debug.Log("Loaded projects: " + result.Data.Length);
}
else
{
    Debug.LogError(result.Error.Message);
}
```

## Configuration

`ApiClientConfig` is the central runtime configuration.

| Setting | Purpose |
| --- | --- |
| `BaseUrl` | Prepended to relative endpoints. Absolute URLs are used as-is. |
| `DefaultHeaders` | Headers applied to every request unless overridden. |
| `TimeoutSeconds` | Default request timeout. `0` lets UnityWebRequest use its default behavior. |
| `AuthenticationMode` | Default auth mode for requests using `UseConfigDefault`. |
| `AuthProvider` | Optional ScriptableObject token provider. |
| `JsonSerializerSettings` | Newtonsoft JSON options. |
| `DefaultResponseFormat` | Response format fallback. Keep `Auto` for most projects. |
| `CertificateHandlingMode` | TLS/certificate behavior. `DefaultValidation` is the production default. |
| `LoggingMode` | Controls API logging. |
| `LogRawJson` | Logs successful JSON response bodies when enabled. |

```csharp
IApiClient apiClient = ApiClientFactory.Create(apiClientConfig);
```

For tests or small integrations, a runtime config is also possible:

```csharp
ApiClientConfig config = ApiClientConfig.CreateRuntimeDefault();
config.BaseUrl = "https://api.example.com/api/v1";
config.TimeoutSeconds = 30;

IApiClient apiClient = ApiClientFactory.Create(config);
```

Keep `DefaultResponseFormat` on `Auto` unless an entire client should default to
a non-JSON DTO-like response format. `Auto` maps DTOs to JSON, `string` to text,
`byte[]` to bytes, and `Texture2D` to textures. A global non-`Auto` value applies
to DTO-like calls whose request or endpoint still uses `Auto`, so prefer
per-request or per-endpoint `ResponseFormat` overrides for files, text endpoints,
and images.

## Authentication

Authentication is provided through `IApiAuthProvider`. The provider returns the
token only; APIHelper creates the `Authorization: Bearer ...` header. The
following example is project code; APIHelper does not include `ISessionService`.

```csharp
public sealed class ProjectSessionAuthProvider : IApiAuthProvider
{
    private readonly ISessionService sessionService;

    public ProjectSessionAuthProvider(ISessionService sessionService)
    {
        this.sessionService = sessionService;
    }

    public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(sessionService.GetActiveSession()?.AccessToken);
    }
}

IApiClient apiClient = ApiClientFactory.Create(
    apiClientConfig,
    new ProjectSessionAuthProvider(sessionService));
```

For designer-assigned auth providers, derive from `ApiAuthProviderAsset` and
assign it to the `ApiClientConfig` asset.

Requests and endpoints can choose:

- `UseConfigDefault`: follow `ApiClientConfig.AuthenticationMode`.
- `Required`: token must be available.
- `Optional`: token is used if available.
- `Disabled`: never attach auth.

## Integrations

APIHelper does not depend on the other JorisHoef runtime packages.

Other packages or project code can integrate by implementing `IApiAuthProvider`. Session Helper provides an optional `SessionAuthProvider` adapter in its own `SessionHelper.APIHelper` assembly when the `SESSION_HELPER_APIHELPER` scripting define symbol is enabled. That adapter lives in Session Helper, not in APIHelper.

## String Endpoint Workflow

Use string constants for simple routes. This is the most compact workflow and
works well with IDE search/refactoring.

```csharp
public static class ApiRoutes
{
    public const string Login = "auth/login";
    public const string Projects = "projects";
}

ApiResult<ProjectDto[]> result =
    await apiClient.GetAsync<ProjectDto[]>(ApiRoutes.Projects, cancellationToken);
```

POST with body:

```csharp
var request = new CreateProjectRequest { Name = "Site A" };

ApiResult<ProjectDto> result =
    await apiClient.PostAsync<ProjectDto>(ApiRoutes.Projects, request, cancellationToken);
```

## ApiEndpoint Workflow

Use `ApiEndpoint` when method, auth, timeout, headers, or query parameters should
travel with the endpoint definition.

```csharp
public static class ProjectApiEndpoints
{
    public static readonly ApiEndpoint GetAll =
        new ApiEndpoint("projects", HttpMethod.GET, ApiAuthenticationRequirement.Required);

    public static readonly ApiEndpoint Create =
        new ApiEndpoint("projects", HttpMethod.POST, ApiAuthenticationRequirement.Required);

    public static ApiEndpoint GetById(string projectId) =>
        new ApiEndpoint("projects/{id}", HttpMethod.GET, ApiAuthenticationRequirement.Required)
            .WithPathParameter("id", projectId);
}
```

Usage:

```csharp
ApiResult<ProjectDto[]> projects =
    await apiClient.SendAsync<ProjectDto[]>(ProjectApiEndpoints.GetAll, cancellationToken);

ApiResult<ProjectDto> created =
    await apiClient.SendAsync<ProjectDto>(
        ProjectApiEndpoints.Create,
        new CreateProjectRequest { Name = "Site A" },
        cancellationToken);

ApiResult<ProjectDto> project =
    await apiClient.SendAsync<ProjectDto>(
        ProjectApiEndpoints.GetById(projectId),
        cancellationToken);
```

Default headers and query parameters:

```csharp
public static readonly ApiEndpoint Search =
    new ApiEndpoint("projects/search", HttpMethod.GET, ApiAuthenticationRequirement.Required)
        .WithQueryParameter("include", "users")
        .WithHeader("X-Client", "Unity");
```

Path parameters are URL escaped. Missing path parameters fail clearly before a
request is sent.

## ScriptableObject Endpoint Workflow

Create an endpoint asset:

`Assets > Create > JorisHoef > API Helper > Endpoint Definition`

Configure:

- Display name
- Path
- Method
- Authentication
- Timeout override
- Default headers
- Default query parameters

Use it from a MonoBehaviour or service:

```csharp
using System.Threading;
using System.Threading.Tasks;
using JorisHoef.APIHelper.Configuration;
using JorisHoef.APIHelper.Core;
using JorisHoef.APIHelper.Models;
using UnityEngine;

public sealed class ProjectListController : MonoBehaviour
{
    [SerializeField] private ApiClientConfig apiClientConfig;
    [SerializeField] private ApiEndpointDefinition getProjectsEndpoint;

    private IApiClient apiClient;

    private void Awake()
    {
        apiClient = ApiClientFactory.Create(apiClientConfig);
    }

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        ApiResult<ProjectDto[]> result =
            await apiClient.SendAsync<ProjectDto[]>(getProjectsEndpoint, cancellationToken);

        if (!result.IsSuccess)
        {
            Debug.LogError(result.Error.Message);
        }
    }
}
```

`ApiEndpointDefinition` converts to the same `ApiEndpoint` model used by
code-defined endpoints. It does not store runtime state.

## Advanced ApiRequest Workflow

Use `ApiRequest` when one call needs custom details that do not belong in a
route constant or endpoint definition.

```csharp
var request = new ApiRequest(
    "projects",
    HttpMethod.GET,
    ApiAuthenticationRequirement.Required);

request.QueryParameters["include"] = "users";
request.Headers["X-Client"] = "Unity";
request.TimeoutSeconds = 10;

ApiResult<ProjectDto[]> result =
    await apiClient.SendAsync<ProjectDto[]>(request, cancellationToken);
```

POST raw or multipart content by setting `Body`, `BodyFormat`, and headers as
needed. Use `ResponseFormat` when you want to override the format inferred from
`TResponse`.

## Media Types

APIHelper officially supports these response shapes:

- JSON DTOs
- `string`
- `byte[]`
- `Texture2D`

Audio and video do not have dedicated built-in handlers. Download audio/video
as `byte[]` and let the consuming application decide how to store, stream, or
decode it.

### Response Formats

`ApiResponseFormat.Auto` is the default:

- DTO response types use JSON.
- `string` uses text.
- `byte[]` uses raw bytes.
- `Texture2D` uses `DownloadHandlerTexture`.

Recommended usage:

| Response type | Recommended format |
| --- | --- |
| DTO objects/arrays | `Auto` or `Json` |
| `string` | `Auto` or `Text` |
| `byte[]` | `Auto` or `Bytes` |
| `Texture2D` | `Auto` or `Texture` |

```csharp
ApiResult<ProjectDto[]> projects =
    await apiClient.GetAsync<ProjectDto[]>(ApiRoutes.Projects, cancellationToken);

ApiResult<string> health =
    await apiClient.GetAsync<string>(StatusEndpoints.Health, cancellationToken);

ApiResult<byte[]> file =
    await apiClient.GetAsync<byte[]>(FileEndpoints.Download, cancellationToken);

ApiResult<Texture2D> avatar =
    await apiClient.GetAsync<Texture2D>(ImageEndpoints.Avatar, cancellationToken);
```

Use an advanced request when the response format should be explicit:

```csharp
var request = new ApiRequest("reports/latest.pdf", HttpMethod.GET)
{
    ResponseFormat = ApiResponseFormat.Bytes
};

ApiResult<byte[]> result = await apiClient.SendAsync<byte[]>(request, cancellationToken);
```

Texture responses use Unity's `DownloadHandlerTexture`. APIHelper preserves the
HTTP status code, response headers, raw bytes, and any useful text body Unity
exposes for failed texture requests. Some servers return JSON error bodies for
image endpoints, but `DownloadHandlerTexture` does not guarantee rich text error
access. If you need reliable structured error parsing for a media endpoint,
request `byte[]` or `string` for that workflow and decode the media in project
code.

### Request Body Formats

JSON remains the default:

```csharp
ApiResult<ProjectDto> result =
    await apiClient.PostAsync<ProjectDto>(
        "projects",
        new CreateProjectRequest { Name = "Site A" },
        cancellationToken);
```

Raw text body:

```csharp
var request = new ApiRequest("status", HttpMethod.POST)
{
    Body = "ready",
    BodyFormat = ApiRequestBodyFormat.RawText,
    ResponseFormat = ApiResponseFormat.Text
};

ApiResult<string> result = await apiClient.SendAsync<string>(request, cancellationToken);
```

Raw byte body:

```csharp
var request = new ApiRequest("files/upload", HttpMethod.POST)
{
    Body = fileBytes,
    BodyFormat = ApiRequestBodyFormat.RawBytes,
    ResponseFormat = ApiResponseFormat.Json
};

ApiResult<UploadResultDto> result =
    await apiClient.SendAsync<UploadResultDto>(request, cancellationToken);
```

Multipart upload:

```csharp
using UnityEngine.Networking;

var sections = new List<IMultipartFormSection>
{
    new MultipartFormDataSection("description", "Profile image"),
    new MultipartFormFileSection("avatar", imageBytes, "avatar.png", "image/png")
};

var request = new ApiRequest("profile/avatar", HttpMethod.POST)
{
    Body = sections,
    BodyFormat = ApiRequestBodyFormat.MultipartForm
};

ApiResult<ProfileDto> result =
    await apiClient.SendAsync<ProfileDto>(request, cancellationToken);
```

### Headers

APIHelper applies these default `Accept` headers when callers do not provide
one:

| Response format | Default `Accept` |
| --- | --- |
| JSON | `application/json` |
| Text | `text/plain,*/*` |
| Bytes | `*/*` |
| Texture | `image/png,image/jpeg,image/webp,*/*` |

Default `Content-Type` values are:

| Body format | Default `Content-Type` |
| --- | --- |
| JSON | `application/json` |
| RawText | `text/plain` |
| RawBytes | `application/octet-stream` |
| MultipartForm | Unity-generated `multipart/form-data` with boundary |

Explicit headers are never overwritten. You can set `Accept` or `Content-Type`
through `ApiClientConfig.DefaultHeaders`, `ApiEndpoint` default headers, or
`ApiRequest.Headers`.

```csharp
var request = new ApiRequest("exports/csv", HttpMethod.POST)
{
    Body = csv,
    BodyFormat = ApiRequestBodyFormat.RawText,
    ResponseFormat = ApiResponseFormat.Bytes
};

request.Headers["Content-Type"] = "text/csv";
request.Headers["Accept"] = "application/octet-stream";
```

For custom formats such as CSV, PDF, audio, or video, request `string` or
`byte[]` and parse/decode in your application code.

### Extension Points

The package-level extension point for custom media is intentionally small:
request text or bytes, then parse outside APIHelper. For example, a project can
build its own CSV, PDF, audio, or video helper around `IApiClient.GetAsync<string>`
or `IApiClient.GetAsync<byte[]>` without modifying the package.

APIHelper does not currently expose a built-in `IApiResponseHandler<T>` registry.
That keeps the package lightweight and avoids a framework-style plugin system.
Add such an abstraction only if project code repeatedly needs custom response
decoders that cannot be handled cleanly with `string` or `byte[]`.

## Versioning

Current package version: `1.0.0`.

Branch strategy:

- `main`: stable package branch.
- `develop`: development package branch.
- Stable release tags should use names such as `v1.0.0` when published.
- Beta tags may use prerelease versions such as `v1.1.0-beta.1`.

Unity may pin Git package commits in `Packages/packages-lock.json`; to update, open Package Manager and update the package, remove the lock entry, or change the Git ref in `manifest.json`.

## CI And Releases

GitHub Actions validates package structure on pull requests and pushes to
`develop` or `main`. The validation checks package metadata, asmdefs, required
docs, sample placement, and banned Unity-generated artifacts.

Unity EditMode tests are available through the manual `Unity EditMode Tests`
workflow. Enable it by configuring the repository secrets required by
GameCI/Unity:

- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`

Unity Asset Store publishing is not part of this package workflow. It can be
added later as a separate release channel.

## Error Handling

All client calls return `ApiResult<T>`.

```csharp
ApiResult<ProjectDto> result = await apiClient.SendAsync<ProjectDto>(endpoint, cancellationToken);

if (result.IsSuccess)
{
    ProjectDto project = result.Data;
}
else
{
    ApiError error = result.Error;

    Debug.LogError(error.Message);
    Debug.Log("Status: " + error.HttpStatusCode);
    Debug.Log("URL: " + error.RequestUrl);
    Debug.Log("Raw body: " + error.RawResponseBody);
    if (error.ResponseHeaders.TryGetValue("X-Trace-Id", out string traceId))
    {
        Debug.Log("Trace header: " + traceId);
    }

    if (error.HasValidationErrors)
    {
        foreach (KeyValuePair<string, string[]> pair in error.ValidationErrors)
        {
            Debug.Log(pair.Key + ": " + string.Join(", ", pair.Value));
        }
    }
}
```

`ApiError` includes:

- Success/failure message
- HTTP status code
- Request URL
- Raw response body
- Response headers
- Backend code/message when available
- Validation errors when available
- Exception details when relevant
- Cancellation and timeout flags

`ApiTransportResponse` is internal package plumbing between the Unity transport
and response parsers. Application code should use `ApiResult<T>` and `ApiError`
for success and failure details.

## Legacy ApiServices Migration

`ApiServices` remains as a legacy/convenience facade. It delegates to a
process-wide `IApiClient`, but new code should inject `IApiClient` instead of
using static global state.

Old:

```csharp
ApiCallResult<ProjectDto[]> result =
    await ApiServices.GetAsync<ProjectDto[]>("projects", true, accessToken);
```

New:

```csharp
IApiClient apiClient = ApiClientFactory.Create(apiClientConfig, authProvider);

ApiResult<ProjectDto[]> result =
    await apiClient.SendAsync<ProjectDto[]>(ProjectApiEndpoints.GetAll, cancellationToken);
```

Temporary facade migration:

```csharp
ApiServices.Configure(apiClient);

ApiResult<ProjectDto[]> result =
    await ApiServices.SendAsync<ProjectDto[]>(ProjectApiEndpoints.GetAll, cancellationToken);
```

Obsolete `ApiServices` overloads that take `requiresAuthentication` and
`accessToken` still call the new pipeline, but should be replaced.

Legacy byte and texture download helpers are compatibility wrappers around the
configured `IApiClient`. Prefer:

```csharp
ApiResult<byte[]> bytes = await apiClient.GetAsync<byte[]>(FileEndpoints.Download, cancellationToken);
ApiResult<Texture2D> texture = await apiClient.GetAsync<Texture2D>(ImageEndpoints.Avatar, cancellationToken);
```

## Limitations

- APIHelper uses UnityWebRequest as its built-in transport.
- Built-in response handling covers JSON DTOs, `string`, `byte[]`, and `Texture2D`.
- Audio, video, PDF, CSV, archive, and other custom formats should be downloaded as `byte[]` or `string` and decoded by application code.
- There is no built-in response-handler registry.
- `ApiServices` remains for legacy and simple facade use, but new services should depend on `IApiClient`.
- Certificate bypass is intended only for explicit development scenarios; keep production configs on `DefaultValidation`.

## Complete Example

```csharp
using System.Threading;
using System.Threading.Tasks;
using JorisHoef.APIHelper;
using JorisHoef.APIHelper.Authentication;
using JorisHoef.APIHelper.Configuration;
using JorisHoef.APIHelper.Core;
using JorisHoef.APIHelper.Models;
using UnityEngine;

public sealed class ProjectsExample : MonoBehaviour
{
    [SerializeField] private ApiClientConfig apiClientConfig;

    private IApiClient apiClient;

    private static class Endpoints
    {
        public static readonly ApiEndpoint List =
            new ApiEndpoint("projects", HttpMethod.GET, ApiAuthenticationRequirement.Required);

        public static readonly ApiEndpoint Create =
            new ApiEndpoint("projects", HttpMethod.POST, ApiAuthenticationRequirement.Required);
    }

    private void Awake()
    {
        apiClient = ApiClientFactory.Create(apiClientConfig, new ExampleTokenProvider());
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        ApiResult<ProjectDto[]> listResult =
            await apiClient.SendAsync<ProjectDto[]>(Endpoints.List, cancellationToken);

        if (listResult.IsFailure)
        {
            Debug.LogError(listResult.Error.Message);
            return;
        }

        var body = new CreateProjectRequest { Name = "Site A" };
        ApiResult<ProjectDto> createResult =
            await apiClient.SendAsync<ProjectDto>(Endpoints.Create, body, cancellationToken);

        if (createResult.IsSuccess)
        {
            Debug.Log("Created project: " + createResult.Data.Name);
        }
    }
}

public sealed class ExampleTokenProvider : IApiAuthProvider
{
    public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult("replace-with-session-token");
    }
}

public sealed class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public sealed class CreateProjectRequest
{
    public string Name { get; set; }
}
```

## Best Practices

- Depend on `IApiClient` in application services.
- Keep `ApiServices` only for legacy migration or very small projects.
- Use string constants for simple endpoints.
- Use `ApiEndpoint` when route metadata matters.
- Use `ApiEndpointDefinition` when designers/non-developers need inspector
  configuration.
- Use `ApiRequest` only for advanced one-off custom calls.
- Use `byte[]` for audio, video, PDFs, archives, and other binary payloads.
- Set explicit `Accept`/`Content-Type` headers only when an API requires them.
- Keep tokens in an `IApiAuthProvider`; do not pass tokens through every call.
- Prefer relative paths plus `ApiClientConfig.BaseUrl`.
- Keep certificate handling on `DefaultValidation` for production.
- Pass `CancellationToken` from MonoBehaviour lifetime, UI flow, or service flow.

## FAQ / Troubleshooting

### Why did my relative endpoint fail?

Check `ApiClientConfig.BaseUrl`. Relative endpoints are joined to this value.
Absolute URLs skip the base URL.

### Why is no Authorization header sent?

Check three things:

- The request or endpoint auth mode is `Required`, `Optional`, or `UseConfigDefault`
  with config auth set to `BearerToken`.
- The config has an `ApiAuthProviderAsset`, or a code `IApiAuthProvider` was
  passed to `ApiClientFactory.Create`.
- The provider returns a non-empty token.

### Why does a path like `projects/{id}` throw before sending?

`ApiEndpoint` requires path parameters to be resolved before request creation.
Use `.WithPathParameter("id", value)`.

### Can designers configure endpoints?

Yes. Use `ApiEndpointDefinition` assets. They convert to `ApiEndpoint` at
runtime and share the same request pipeline.

### Can APIHelper download audio or video?

Yes, as bytes. APIHelper intentionally does not include built-in `AudioClip` or
`VideoPlayer` integrations. Download the payload with `GetAsync<byte[]>`, then
let your application decode, stream, cache, or pass it to a media-specific
system.

### Why is `RawJson` obsolete?

`ApiResult<T>.RawJson` was kept for compatibility, but APIHelper now supports
text, bytes, and textures too. Use `RawResponseBody` for response text.

### Can I bypass certificates for local development?

Only explicitly through `BypassInDevelopmentOnly`. This mode returns a bypass
handler only in the Unity Editor or development builds. Production defaults to
normal certificate validation.

### Why are old ApiServices calls marked obsolete?

They pass auth decisions and tokens per call, which makes APIs noisy and harder
to configure safely. Use `IApiClient` with config/auth providers instead.

## Documentation Maintenance Policy

Documentation must be updated whenever public APIs are added, changed,
deprecated, or removed. This includes:

- README examples and migration notes.
- XML documentation on public types and members.
- Sample scripts or scenes when public workflow behavior changes.
- Obsolete messages that point to the current replacement API.
- Contributor guidance in `CONTRIBUTING.md` when process expectations change.
