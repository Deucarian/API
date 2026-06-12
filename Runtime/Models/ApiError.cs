using System;
using System.Collections.Generic;

namespace Deucarian.API.Models
{
    /// <summary>
    /// Structured failure details returned by <see cref="ApiResult{T}"/> when a request fails.
    /// </summary>
    public sealed class ApiError
    {
        /// <summary>Human-readable failure message suitable for logs or basic UI.</summary>
        public string Message { get; set; }

        /// <summary>HTTP status code when the server returned a response.</summary>
        public long? HttpStatusCode { get; set; }

        /// <summary>Final request URL used by the transport layer.</summary>
        public string RequestUrl { get; set; }

        /// <summary>Raw response body returned by the server when available.</summary>
        public string RawResponseBody { get; set; }

        /// <summary>Response headers returned by the server when available.</summary>
        public Dictionary<string, string> ResponseHeaders { get; set; } =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Backend-specific error code parsed from the response when available.</summary>
        public string BackendCode { get; set; }

        /// <summary>Backend-specific error message parsed from the response when available.</summary>
        public string BackendMessage { get; set; }

        /// <summary>Validation errors parsed from backend error payloads.</summary>
        public Dictionary<string, string[]> ValidationErrors { get; set; } =
                new Dictionary<string, string[]>();

        /// <summary>Exception captured during auth, transport, parsing, cancellation, or timeout when available.</summary>
        public Exception Exception { get; set; }

        /// <summary>Transport-layer error text reported by UnityWebRequest when available.</summary>
        public string TransportError { get; set; }

        /// <summary>True when the request failed because the provided CancellationToken was canceled.</summary>
        public bool IsCancellation { get; set; }

        /// <summary>True when the request appears to have failed due to timeout.</summary>
        public bool IsTimeout { get; set; }

        /// <summary>True when one or more validation errors were parsed.</summary>
        public bool HasValidationErrors =>
                ValidationErrors != null && ValidationErrors.Count > 0;
    }
}
