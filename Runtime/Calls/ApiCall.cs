using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JorisHoef.APIHelper.Models;
using JorisHoef.APIHelper.Services;

namespace JorisHoef.APIHelper.Calls
{
    internal class ApiCall<TResponse>
    {
        protected readonly string _url;
        protected readonly HttpMethod _method;
        protected readonly object _data;
        protected ApiRequestBodyFormat _bodyFormat = ApiRequestBodyFormat.Json;
        private readonly bool _requiresAuthentication;
        private readonly string _accessToken;

        protected ApiCall(string url, HttpMethod method, bool requiresAuthentication, object data)
        {
            _url = url;
            _method = method;
            _data = data;
            _requiresAuthentication = requiresAuthentication;
        }

        protected ApiCall(string url,
                          HttpMethod method,
                          bool requiresAuthentication,
                          object data,
                          string accessToken)
                : this(url, method, requiresAuthentication, data)
        {
            _accessToken = accessToken;
        }

        public static ApiCall<TCallResponse> GetApiCall<TCallResponse>(string url,
                                                                       HttpMethod method,
                                                                       bool requiresAuthentication,
                                                                       object data,
                                                                       string accessToken = null)
        {
            return new ApiCall<TCallResponse>(url, method, requiresAuthentication, data, accessToken);
        }

        public Task<ApiCallResult<TResponse>> Execute(Dictionary<string, string> customHeaders = null)
        {
            return ApiServices.ExecuteLegacyAsync<TResponse>(_url,
                                                             _method,
                                                             _requiresAuthentication,
                                                             _data,
                                                             _accessToken,
                                                             customHeaders,
                                                             _bodyFormat,
                                                             CancellationToken.None);
        }
    }
}
