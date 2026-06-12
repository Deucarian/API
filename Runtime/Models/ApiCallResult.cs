namespace Deucarian.API.Models
{
    /// <summary>
    /// Legacy API result shape. Use ApiResult&lt;T&gt; for new code.
    /// </summary>
    /// <typeparam name="T">The type of data returned in the API call result.</typeparam>
    public class ApiCallResult<T> : ApiResult<T>
    {
        /// <summary>
        /// Converts the modern result type into the legacy result type used by older ApiServices overloads.
        /// </summary>
        /// <param name="result">Modern API result.</param>
        /// <returns>A legacy result with matching data and error metadata.</returns>
        public static ApiCallResult<T> From(ApiResult<T> result)
        {
            if (result == null)
            {
                return null;
            }

            return new ApiCallResult<T>
            {
                    IsSuccess = result.IsSuccess,
                    Data = result.Data,
                    Error = result.Error,
                    HttpMethod = result.HttpMethod,
                    HttpStatusCode = result.HttpStatusCode,
                    RequestUrl = result.RequestUrl,
                    RawResponseBody = result.RawResponseBody
            };
        }
    }
}
