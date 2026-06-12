using Deucarian.API.Models;

namespace Deucarian.API.Core
{
    internal interface IApiLogger
    {
        void LogRequest(ApiRequest request, string requestUrl);
        void LogResponse<TResponse>(ApiResult<TResponse> result);
        void LogError(ApiError error);
    }
}
