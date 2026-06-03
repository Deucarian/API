using UnityEngine.Networking;

namespace JorisHoef.APIHelper.Certificates
{
    internal sealed class BypassCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certData)
        {
            return true;
        }
    }
}
