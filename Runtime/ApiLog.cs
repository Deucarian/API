using Deucarian.Logging;

namespace Deucarian.API
{
    /// <summary>
    /// Package-level log categories for Deucarian API.
    /// </summary>
    public static class ApiLog
    {
        public static readonly DLog General = DLog.For("Api");
        public static readonly DLog Requests = DLog.For("Api.Requests");
        public static readonly DLog Authentication = DLog.For("Api.Authentication");
        public static readonly DLog Certificates = DLog.For("Api.Certificates");
        public static readonly DLog Samples = DLog.For("Api.Samples");
    }
}
