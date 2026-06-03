using System;

namespace JorisHoef.APIHelper.Authentication
{
    /// <summary>
    /// Exception raised when a request requires authentication but no usable token is available.
    /// </summary>
    public sealed class ApiAuthenticationException : Exception
    {
        /// <summary>
        /// Creates an authentication exception with a caller-facing message.
        /// </summary>
        /// <param name="message">Description of the authentication failure.</param>
        public ApiAuthenticationException(string message) : base(message) { }
    }
}
