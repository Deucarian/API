using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine.Networking;

namespace Deucarian.API.Certificates
{
    internal sealed class DefaultLikeCertificateHandler : CertificateHandler
    {
        private readonly bool _revocation;

        public DefaultLikeCertificateHandler(bool performRevocationCheck = false)
        {
            _revocation = performRevocationCheck;
        }

        protected override bool ValidateCertificate(byte[] certificateData)
        {
            X509Certificate2 cert = null;
            try
            {
                cert = new X509Certificate2(certificateData);

                using (X509Chain chain = new X509Chain())
                {
                    chain.ChainPolicy.RevocationMode =
                            _revocation ? X509RevocationMode.Online : X509RevocationMode.NoCheck;
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                    chain.ChainPolicy.UrlRetrievalTimeout = TimeSpan.FromSeconds(5);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                    chain.ChainPolicy.VerificationTime = DateTime.UtcNow;

                    bool ok = chain.Build(cert);
                    if (!ok)
                    {
                        LogChainDiagnostics("TLS VALIDATION FAILED", cert, chain);
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                ApiLog.Certificates.Warning("[TLS] Exception during validation: " + ex);
                return false;
            }
            finally
            {
                cert?.Dispose();
            }
        }

        private static void LogChainDiagnostics(string title, X509Certificate2 leaf, X509Chain chain)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[TLS] " + title);
            sb.AppendLine("[TLS] Leaf Subject   : " + leaf.Subject);
            sb.AppendLine("[TLS] Leaf Issuer    : " + leaf.Issuer);
            sb.AppendLine("[TLS] NotBefore/After: " + leaf.NotBefore.ToUniversalTime().ToString("u")
                          + " - " + leaf.NotAfter.ToUniversalTime().ToString("u"));
            sb.AppendLine("[TLS] Serial         : " + leaf.SerialNumber);
            sb.AppendLine("[TLS] Thumbprint     : " + leaf.Thumbprint);

            TryAppendSubjectAlternativeNames(sb, leaf);
            AppendChainStatus(sb, chain);
            AppendChainElements(sb, chain);
            TryAppendTruncatedPem(sb, leaf);

            ApiLog.Certificates.Warning(sb.ToString());
        }

        private static void TryAppendSubjectAlternativeNames(StringBuilder sb, X509Certificate2 leaf)
        {
            try
            {
                X509Extension san = leaf.Extensions["2.5.29.17"];
                if (san != null)
                {
                    string formatted = new AsnEncodedData(san.Oid, san.RawData).Format(true);
                    sb.AppendLine("[TLS] SAN            : " + formatted?.Replace('\n', ' ').Trim());
                    return;
                }

                string cn = leaf.GetNameInfo(X509NameType.DnsName, false);
                if (!string.IsNullOrEmpty(cn))
                {
                    sb.AppendLine("[TLS] CN             : " + cn);
                }
            }
            catch
            {
                // Best-effort diagnostics only.
            }
        }

        private static void AppendChainStatus(StringBuilder sb, X509Chain chain)
        {
            if (chain.ChainStatus == null || chain.ChainStatus.Length == 0)
            {
                return;
            }

            sb.AppendLine("[TLS] ChainStatus    :");
            foreach (X509ChainStatus status in chain.ChainStatus)
            {
                sb.AppendLine("[TLS]   - " + status.Status + " | " + status.StatusInformation?.Trim());
            }
        }

        private static void AppendChainElements(StringBuilder sb, X509Chain chain)
        {
            sb.AppendLine("[TLS] Chain Elements :");
            for (int i = 0; i < chain.ChainElements.Count; i++)
            {
                X509ChainElement element = chain.ChainElements[i];
                sb.AppendLine("[TLS]   [" + i + "] Subject: " + element.Certificate.Subject);
                sb.AppendLine("[TLS]       Issuer : " + element.Certificate.Issuer);
                sb.AppendLine("[TLS]       Valid  : "
                              + element.Certificate.NotBefore.ToUniversalTime().ToString("u")
                              + " - "
                              + element.Certificate.NotAfter.ToUniversalTime().ToString("u"));

                if (element.ChainElementStatus == null || element.ChainElementStatus.Length == 0)
                {
                    continue;
                }

                foreach (X509ChainStatus status in element.ChainElementStatus)
                {
                    sb.AppendLine("[TLS]       Status : " + status.Status + " | "
                                  + status.StatusInformation?.Trim());
                }
            }
        }

        private static void TryAppendTruncatedPem(StringBuilder sb, X509Certificate2 leaf)
        {
            try
            {
                string b64 = Convert.ToBase64String(leaf.Export(X509ContentType.Cert));
                sb.AppendLine("[TLS] Leaf PEM (truncated): -----BEGIN CERTIFICATE-----");
                sb.AppendLine("[TLS] "
                              + InsertLineBreaks(b64, 64)
                                      .Split('\n')
                                      .Take(4)
                                      .Aggregate((a, b) => a + '\n' + b));
                sb.AppendLine("[TLS] ... (truncated) ...");
            }
            catch
            {
                // Best-effort diagnostics only.
            }
        }

        private static string InsertLineBreaks(string value, int every)
        {
            StringBuilder sb = new StringBuilder(value.Length + value.Length / every + 10);
            for (int i = 0; i < value.Length; i += every)
            {
                int len = Math.Min(every, value.Length - i);
                sb.AppendLine(value.Substring(i, len));
            }

            return sb.ToString();
        }
    }
}
