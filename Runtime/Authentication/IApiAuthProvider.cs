using System.Threading;
using System.Threading.Tasks;

namespace JorisHoef.APIHelper.Authentication
{
    /// <summary>
    /// Provides access tokens for authenticated API requests.
    /// Implement this in code, or derive from <see cref="ApiAuthProviderAsset"/> for ScriptableObject assignment.
    /// </summary>
    /// <example>
    /// <code>
    /// public Task&lt;string&gt; GetAccessTokenAsync(CancellationToken cancellationToken)
    /// {
    ///     cancellationToken.ThrowIfCancellationRequested();
    ///     return Task.FromResult(session.AccessToken);
    /// }
    /// </code>
    /// </example>
    public interface IApiAuthProvider
    {
        /// <summary>
        /// Returns an access token for an authenticated request.
        /// APIHelper adds the <c>Authorization: Bearer</c> header when a token is returned.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel token retrieval.</param>
        /// <returns>The bearer token without the <c>Bearer</c> prefix, or null/empty when no token is available.</returns>
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken);
    }
}
