# Simple usage

Copy the reference example into your project, add SimpleUsageExample and assign its scoped host references. Its serialized definition fields use the same typed keys as code.

Configure the host once with `ApiEndpointBinding.For(Endpoints.Profile, client, request => new ApiRequest("profile"))`. The application owns the client, authentication and disposal. The endpoint key fixes both payload types. Transport failures remain ApiResult values; destroying the host cancels its requests.

Definitions are authored once in SampleDefinitions.cs where applicable; the caller never invents an ID. Replace the sample set with your project's central definitions. A selected key proves its identity and payload type; startup still needs to bind that definition in the correct scope. Missing configuration reports how to fix it. Dynamic targets and choices are issued by their owner instead of selected from a definition dropdown.
```csharp
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Core;
using Deucarian.API.Models;
namespace Deucarian.API.Samples.SimpleUsage
{
    public sealed class SimpleUsageExample : MonoBehaviour
    {
        [SerializeField] private ApiHost api;
        [SerializeField] private EndpointKey<ProfileRequest, ProfileResponse> endpoint = Endpoints.Profile;
        public Task<ApiResult<ProfileResponse>> LoadProfileAsync(CancellationToken cancellationToken = default) =>
            api.SendAsync(endpoint, new ProfileRequest(), cancellationToken);
    }
}
```
