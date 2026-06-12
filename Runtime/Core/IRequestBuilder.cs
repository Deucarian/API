using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Models;
using UnityEngine.Networking;

namespace Deucarian.API.Core
{
    internal interface IRequestBuilder
    {
        Task<UnityWebRequest> BuildAsync(ApiRequest request,
                                         ApiResponseFormat responseFormat,
                                         CancellationToken cancellationToken);
    }
}
