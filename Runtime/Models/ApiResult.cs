using System;
using System.Collections.Generic;

namespace JorisHoef.APIHelper.Models
{
    /// <summary>
    /// Represents the outcome of an API request.
    /// Successful results expose <see cref="Data"/>. Failed results expose <see cref="Error"/>.
    /// </summary>
    /// <typeparam name="T">Response DTO type expected by the caller.</typeparam>
    /// <example>
    /// <code>
    /// if (result.IsSuccess)
    /// {
    ///     ProjectDto project = result.Data;
    /// }
    /// else
    /// {
    ///     Debug.LogError(result.Error.Message);
    /// }
    /// </code>
    /// </example>
    public class ApiResult<T>
    {
        private ApiError _error;

        /// <summary>True when the HTTP request and response parsing succeeded.</summary>
        public bool IsSuccess { get; set; }

        /// <summary>True when the request failed, was canceled, timed out, or could not be parsed.</summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>Parsed response data for successful requests.</summary>
        public T Data { get; set; }

        /// <summary>Structured failure information for unsuccessful requests.</summary>
        public ApiError Error
        {
            get => _error;
            set
            {
                _error = value;
                ApplyErrorMetadata(value);
            }
        }

        /// <summary>Convenience message for legacy callers. Prefer <see cref="Error"/> in new code.</summary>
        public string ErrorMessage
        {
            get => _error?.Message;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (_error != null)
                    {
                        _error.Message = value;
                    }

                    return;
                }

                EnsureError().Message = value;
            }
        }

        /// <summary>Exception captured during transport, parsing, auth, timeout, or cancellation when available.</summary>
        public Exception Exception
        {
            get => _error?.Exception;
            set
            {
                if (value == null)
                {
                    if (_error != null)
                    {
                        _error.Exception = null;
                    }

                    return;
                }

                EnsureError().Exception = value;
            }
        }

        /// <summary>HTTP method used by the request.</summary>
        public HttpMethod HttpMethod { get; set; }

        /// <summary>HTTP status code returned by the server when available.</summary>
        public long? HttpStatusCode { get; set; }

        /// <summary>Final request URL used by the transport layer.</summary>
        public string RequestUrl { get; set; }

        private string _rawResponseBody;

        /// <summary>
        /// Raw response body. Kept for legacy naming compatibility.
        /// Use <see cref="RawResponseBody"/> for new code.
        /// </summary>
        [Obsolete("Use RawResponseBody. APIHelper now supports text, bytes, and textures in addition to JSON.")]
        public string RawJson
        {
            get => _rawResponseBody;
            set => _rawResponseBody = value;
        }

        /// <summary>Raw response body.</summary>
        public string RawResponseBody
        {
            get => _rawResponseBody;
            set => _rawResponseBody = value;
        }

        /// <summary>Backend-specific error code when one was parsed.</summary>
        public string BackendErrorCode => _error?.BackendCode;

        /// <summary>Backend-specific error message when one was parsed.</summary>
        public string BackendErrorMessage => _error?.BackendMessage;

        /// <summary>Backend validation errors when one or more field errors were parsed.</summary>
        public IReadOnlyDictionary<string, string[]> ValidationErrors => _error?.ValidationErrors;

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="data">Parsed response data.</param>
        /// <param name="httpMethod">HTTP method used by the request.</param>
        /// <param name="httpStatusCode">HTTP status code returned by the server.</param>
        /// <param name="requestUrl">Final request URL.</param>
        /// <param name="rawResponseBody">Raw response body.</param>
        /// <returns>A successful result.</returns>
        public static ApiResult<T> Success(T data,
                                           HttpMethod httpMethod,
                                           long? httpStatusCode,
                                           string requestUrl,
                                           string rawResponseBody)
        {
            return new ApiResult<T>
            {
                    IsSuccess = true,
                    Data = data,
                    HttpMethod = httpMethod,
                    HttpStatusCode = httpStatusCode,
                    RequestUrl = requestUrl,
                    RawResponseBody = rawResponseBody
            };
        }

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="error">Structured error details.</param>
        /// <param name="httpMethod">HTTP method used by the request.</param>
        /// <returns>A failed result.</returns>
        public static ApiResult<T> Failure(ApiError error, HttpMethod httpMethod)
        {
            return new ApiResult<T>
            {
                    IsSuccess = false,
                    HttpMethod = httpMethod,
                    Error = error
            };
        }

        protected ApiError EnsureError()
        {
            if (_error == null)
            {
                _error = new ApiError();
            }

            return _error;
        }

        private void ApplyErrorMetadata(ApiError error)
        {
            if (error == null)
            {
                return;
            }

            HttpStatusCode = error.HttpStatusCode;
            RequestUrl = error.RequestUrl;
            RawResponseBody = error.RawResponseBody;
        }
    }
}
