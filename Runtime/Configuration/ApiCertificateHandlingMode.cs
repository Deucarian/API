namespace JorisHoef.APIHelper.Configuration
{
    /// <summary>
    /// Certificate validation behavior for requests created from <see cref="ApiClientConfig"/>.
    /// </summary>
    public enum ApiCertificateHandlingMode
    {
        /// <summary>Use Unity/platform certificate validation. This is the safe production default.</summary>
        DefaultValidation,

        /// <summary>Use the package default custom validation handler when available.</summary>
        CustomDefaultValidation,

        /// <summary>Bypass certificate validation only in the Unity Editor or development builds.</summary>
        BypassInDevelopmentOnly
    }
}
