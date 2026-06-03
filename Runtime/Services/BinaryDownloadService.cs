using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JorisHoef.APIHelper.Models;

namespace JorisHoef.APIHelper.Services
{
    internal sealed class BinaryDownloadService
    {
        public async Task<ApiCallResult<byte[]>> ExecuteAsync(
                string endpoint,
                string accessToken = null,
                Dictionary<string, string> customHeaders = null,
                CancellationToken cancellationToken = default)
        {
            ApiRequest request = new ApiRequest(endpoint,
                                                HttpMethod.GET,
                                                string.IsNullOrWhiteSpace(accessToken)
                                                        ? ApiAuthenticationRequirement.Disabled
                                                        : ApiAuthenticationRequirement.Required)
            {
                    BearerTokenOverride = accessToken,
                    ResponseFormat = ApiResponseFormat.Bytes
            };

            ApplyHeaders(request, customHeaders);
            ApiResult<byte[]> result = await ApiServices.Client.SendAsync<byte[]>(request, cancellationToken);
            return ApiCallResult<byte[]>.From(result);
        }

        private static void ApplyHeaders(ApiRequest request, Dictionary<string, string> customHeaders)
        {
            if (customHeaders == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> header in customHeaders)
            {
                if (!string.IsNullOrWhiteSpace(header.Key))
                {
                    request.Headers[header.Key] = header.Value;
                }
            }
        }
    }
}
