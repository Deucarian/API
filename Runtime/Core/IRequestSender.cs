using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Models;
using UnityEngine.Networking;

namespace Deucarian.API.Core
{
    internal interface IRequestSender
    {
        Task<ApiTransportResponse> SendAsync(UnityWebRequest request,
                                             CancellationToken cancellationToken);
    }
}
