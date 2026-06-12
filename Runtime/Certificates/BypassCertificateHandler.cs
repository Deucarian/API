using UnityEngine.Networking;

namespace Deucarian.API.Certificates
{
    internal sealed class BypassCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certData)
        {
            return true;
        }
    }
}
