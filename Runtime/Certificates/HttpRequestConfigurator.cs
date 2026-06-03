using System.Collections.Generic;
using UnityEngine.Networking;

namespace JorisHoef.APIHelper.Certificates
{
    /// <summary>
    /// Legacy request setup helper used by older download and ApiCall paths.
    /// New API client requests are configured through ApiClientConfig and ApiRequest.
    /// </summary>
    public static class HttpRequestConfigurator
    {
        #region Public Properties
        /// <summary>
        /// Certificate handler factory used by legacy request helpers.
        /// Defaults to safe certificate validation.
        /// </summary>
        public static ICertificateHandlerFactory CertificateFactory { get; set; } =
            new DefaultCertificateHandlerFactory();
        #endregion

        #region Public Methods
        /// <summary>
        /// Applies legacy certificate, bearer token, accept header, and custom header settings to a request.
        /// </summary>
        /// <param name="request">UnityWebRequest to configure.</param>
        /// <param name="url">Request URL used to choose the certificate handler.</param>
        /// <param name="bearerToken">Optional bearer token without the <c>Bearer</c> prefix.</param>
        /// <param name="customHeaders">Optional additional request headers.</param>
        public static void Apply(UnityWebRequest request,
                                 string url,
                                 string bearerToken = null,
                                 Dictionary<string, string> customHeaders = null)
        {
            CertificateHandler ch = CertificateFactory?.CreateFor(url);
            if (ch != null)
            {
                request.certificateHandler = ch;
                request.disposeCertificateHandlerOnDispose = true;
            }

            if (!string.IsNullOrEmpty(bearerToken))
            {
                request.SetRequestHeader("Authorization", "Bearer " + bearerToken);
            }

            request.SetRequestHeader("Accept", "application/json");

            if (customHeaders != null)
            {
                foreach (KeyValuePair<string, string> kvp in customHeaders)
                {
                    request.SetRequestHeader(kvp.Key, kvp.Value);
                }
            }
        }
        #endregion
    }
}
