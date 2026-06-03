using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JorisHoef.APIHelper.Models;
using UnityEngine;

namespace JorisHoef.APIHelper.Services
{
    internal sealed class TextureDownloadService
    {
        public async Task<ApiCallResult<Texture2D>> ExecuteAsync(
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
                    ResponseFormat = ApiResponseFormat.Texture
            };

            ApplyHeaders(request, customHeaders);
            ApiResult<Texture2D> result =
                    await ApiServices.Client.SendAsync<Texture2D>(request, cancellationToken);
            return ApiCallResult<Texture2D>.From(result);
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
