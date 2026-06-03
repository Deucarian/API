using System.Threading;
using System.Threading.Tasks;
using JorisHoef.APIHelper.Models;
using UnityEngine.Networking;

namespace JorisHoef.APIHelper.Core
{
    internal interface IRequestBuilder
    {
        Task<UnityWebRequest> BuildAsync(ApiRequest request,
                                         ApiResponseFormat responseFormat,
                                         CancellationToken cancellationToken);
    }
}
