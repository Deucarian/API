using System.Threading;
using System.Threading.Tasks;
using JorisHoef.APIHelper.Authentication;
using UnityEngine;

namespace JorisHoef.APIHelper.Samples
{
    /// <summary>
    /// Example-only auth provider.
    /// In a real project, retrieve the token from your session/login service instead of a serialized field.
    /// APIHelper turns the returned token into an Authorization: Bearer header.
    /// </summary>
    [CreateAssetMenu(menuName = "JorisHoef/API Helper/Samples/Example Auth Provider",
                     fileName = "ExampleAuthProvider")]
    public sealed class ExampleAuthProvider : ApiAuthProviderAsset
    {
        [Tooltip("Example token returned by this sample provider. Leave empty to simulate no active session.")]
        [SerializeField] private string exampleAccessToken;

        /// <summary>
        /// Returns the sample token. Replace this with session-service lookup in production code.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel token retrieval.</param>
        /// <returns>The configured sample token.</returns>
        public override Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(exampleAccessToken);
        }
    }
}
