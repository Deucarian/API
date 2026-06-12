using Deucarian.API.Configuration;
using UnityEngine.Networking;

namespace Deucarian.API.Certificates
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
