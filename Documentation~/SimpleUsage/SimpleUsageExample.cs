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
