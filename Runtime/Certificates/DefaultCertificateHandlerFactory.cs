using JorisHoef.APIHelper.Configuration;
using UnityEngine.Networking;

namespace JorisHoef.APIHelper.Certificates
{
    internal sealed class DefaultCertificateHandlerFactory : ICertificateHandlerFactory
    {
        private readonly ApiCertificateHandlerFactory _factory =
                new ApiCertificateHandlerFactory(ApiCertificateHandlingMode.DefaultValidation);

        public CertificateHandler CreateFor(string url)
        {
            return _factory.CreateFor(url);
        }
    }
}
