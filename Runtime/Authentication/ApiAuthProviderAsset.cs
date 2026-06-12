using UnityEngine;

namespace Deucarian.API.Authentication
{
    /// <summary>
    /// ScriptableObject base class for auth providers that designers can assign on ApiClientConfig.
    /// </summary>
    public abstract class ApiAuthProviderAsset : ScriptableObject, IApiAuthProvider
    {
        /// <summary>
        /// Returns an access token for an authenticated request.
        /// API adds the <c>Authorization: Bearer</c> header when a token is returned.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel token retrieval.</param>
        /// <returns>The bearer token without the <c>Bearer</c> prefix, or null/empty when no token is available.</returns>
        public abstract System.Threading.Tasks.Task<string> GetAccessTokenAsync(
                System.Threading.CancellationToken cancellationToken);
    }
}
