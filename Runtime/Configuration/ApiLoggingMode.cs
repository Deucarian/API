namespace JorisHoef.APIHelper.Configuration
{
    /// <summary>
    /// Controls APIHelper request, response, and error logging.
    /// </summary>
    public enum ApiLoggingMode
    {
        /// <summary>Disable APIHelper logging.</summary>
        None,

        /// <summary>Log errors only.</summary>
        ErrorsOnly,

        /// <summary>Log request and response metadata in addition to errors.</summary>
        Verbose
    }
}
