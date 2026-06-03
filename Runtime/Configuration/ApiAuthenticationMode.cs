namespace JorisHoef.APIHelper.Configuration
{
    /// <summary>
    /// Default authentication behavior configured on <see cref="ApiClientConfig"/>.
    /// </summary>
    public enum ApiAuthenticationMode
    {
        /// <summary>Do not attach authentication by default.</summary>
        None,

        /// <summary>Attach an Authorization bearer token from the configured auth provider.</summary>
        BearerToken
    }
}
