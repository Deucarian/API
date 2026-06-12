namespace Deucarian.API.Configuration
{
    /// <summary>
    /// Controls API request, response, and error logging.
    /// </summary>
    public enum ApiLoggingMode
    {
        /// <summary>Disable API logging.</summary>
        None,

        /// <summary>Log errors only.</summary>
        ErrorsOnly,

        /// <summary>Log request and response metadata in addition to errors.</summary>
        Verbose
    }
}
