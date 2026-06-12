using Deucarian.API.Configuration;

namespace Deucarian.API
{
    /// <summary>
    /// Legacy editor/debug toggle for raw JSON logging.
    /// Prefer <c>ApiClientConfig.LoggingMode</c> and <c>ApiClientConfig.LogRawJson</c> for new code.
    /// </summary>
    public static class APIDebugSettings
    {
        /// <summary>
        /// When true, API logs raw JSON response bodies in supported legacy/debug paths.
        /// </summary>
        public static bool LogRawJson;
    }
}
