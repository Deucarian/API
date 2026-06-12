using Deucarian.API.Models;

namespace Deucarian.API.Core
{
    internal interface IApiResponseParser
    {
        ApiResult<TResponse> Parse<TResponse>(ApiRequest request,
                                              ApiTransportResponse response,
                                              ApiResponseFormat responseFormat);
    }
}
