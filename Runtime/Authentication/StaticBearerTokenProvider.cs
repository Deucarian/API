using System.Threading;
using System.Threading.Tasks;

namespace JorisHoef.APIHelper.Authentication
{
    /// <summary>
    /// Simple code-defined bearer token provider, useful for tests or small integrations.
    /// Production apps usually wrap their session service in an IApiAuthProvider instead.
    /// </summary>
    public sealed class StaticBearerTokenProvider : IApiAuthProvider
    {
        /// <summary>
        /// Creates a provider that always returns the supplied token.
        /// </summary>
        /// <param name="accessToken">Bearer token without the <c>Bearer</c> prefix.</param>
        public StaticBearerTokenProvider(string accessToken)
        {
            AccessToken = accessToken;
        }

        /// <summary>Token returned by this provider.</summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Returns the configured static token.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel token retrieval.</param>
        /// <returns>The configured access token.</returns>
        public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(AccessToken);
        }
    }
}
