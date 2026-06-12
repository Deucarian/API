using UnityEngine.Networking;

namespace Deucarian.API.Certificates
{
    /// <summary>
    /// Factory for optional UnityWebRequest certificate handlers.
    /// Returning null keeps Unity/platform certificate validation.
    /// </summary>
    public interface ICertificateHandlerFactory
    {
        #region Public Methods
        /// <summary>
        /// Creates a certificate handler for a request URL.
        /// </summary>
        /// <param name="url">Request URL.</param>
        /// <returns>A certificate handler, or null to keep default validation.</returns>
        CertificateHandler CreateFor(string url);
        #endregion
    }
}
