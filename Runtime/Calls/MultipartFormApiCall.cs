using JorisHoef.APIHelper.Models;

namespace JorisHoef.APIHelper.Calls
{
    internal sealed class MultipartFormApiCall<TResult> : ApiCall<TResult>
    {
        private MultipartFormApiCall(string url, HttpMethod method, bool requiresAuthentication, object data)
                : base(url, method, requiresAuthentication, data)
        {
            _bodyFormat = ApiRequestBodyFormat.MultipartForm;
        }

        private MultipartFormApiCall(string url,
                                     HttpMethod method,
                                     bool requiresAuthentication,
                                     object data,
                                     string accessToken)
                : base(url, method, requiresAuthentication, data, accessToken)
        {
            _bodyFormat = ApiRequestBodyFormat.MultipartForm;
        }

        public new static ApiCall<TCallResponse> GetApiCall<TCallResponse>(string url,
                                                                           HttpMethod method,
                                                                           bool requiresAuthentication,
                                                                           object data,
                                                                           string tokenToSend = null)
        {
            return new MultipartFormApiCall<TCallResponse>(url,
                                                           method,
                                                           requiresAuthentication,
                                                           data,
                                                           tokenToSend);
        }
    }
}
