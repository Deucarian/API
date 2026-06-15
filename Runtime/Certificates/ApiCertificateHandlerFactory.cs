using Deucarian.API.Configuration;
using UnityEngine.Networking;

namespace Deucarian.API.Certificates
{
    /// <summary>
    /// Creates certificate handlers according to an <see cref="ApiCertificateHandlingMode"/>.
    /// Default validation returns null so Unity/platform validation is used.
    /// </summary>
    public sealed class ApiCertificateHandlerFactory : ICertificateHandlerFactory
    {
        private readonly ApiCertificateHandlingMode _mode;
        private bool _alreadyLogged;

        /// <summary>
        /// Creates a certificate handler factory for the given mode.
        /// </summary>
        /// <param name="mode">Certificate handling mode.</param>
        public ApiCertificateHandlerFactory(ApiCertificateHandlingMode mode)
        {
            _mode = mode;
        }

        /// <summary>
        /// Creates a certificate handler for the requested URL, or null to use Unity/platform validation.
        /// </summary>
        /// <param name="url">Request URL.</param>
        /// <returns>A certificate handler, or null for default validation.</returns>
        public CertificateHandler CreateFor(string url)
        {
            switch (_mode)
            {
                case ApiCertificateHandlingMode.CustomDefaultValidation:
                    LogOnce("[ApiCertificateHandlerFactory] Using DefaultLikeCertificateHandler.");
                    return new DefaultLikeCertificateHandler();

                case ApiCertificateHandlingMode.BypassInDevelopmentOnly:
                    return CreateDevelopmentBypassHandler();

                case ApiCertificateHandlingMode.DefaultValidation:
                default:
                    return null;
            }
        }

        private CertificateHandler CreateDevelopmentBypassHandler()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            LogOnce("[ApiCertificateHandlerFactory] Using dev-only BypassCertificateHandler.");
            return new BypassCertificateHandler();
#else
            LogOnce("[ApiCertificateHandlerFactory] Certificate bypass was requested but is disabled outside development builds.");
            return null;
#endif
        }

        private void LogOnce(string message)
        {
            if (_alreadyLogged)
            {
                return;
            }

            ApiLog.Certificates.Info(message);
            _alreadyLogged = true;
        }
    }
}
