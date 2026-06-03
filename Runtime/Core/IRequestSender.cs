using System.Threading;
using System.Threading.Tasks;
using JorisHoef.APIHelper.Models;
using UnityEngine.Networking;

namespace JorisHoef.APIHelper.Core
{
    internal interface IRequestSender
    {
        Task<ApiTransportResponse> SendAsync(UnityWebRequest request,
                                             CancellationToken cancellationToken);
    }
}
