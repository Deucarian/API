using JorisHoef.APIHelper.Models;

namespace JorisHoef.APIHelper.Core
{
    internal interface IApiResponseParser
    {
        ApiResult<TResponse> Parse<TResponse>(ApiRequest request,
                                              ApiTransportResponse response,
                                              ApiResponseFormat responseFormat);
    }
}
