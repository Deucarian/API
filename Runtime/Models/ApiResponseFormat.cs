using UnityEngine;

namespace JorisHoef.APIHelper.Models
{
    /// <summary>
    /// Describes how APIHelper should read a successful response body.
    /// <see cref="Auto"/> keeps the easy path: DTOs use JSON, <see cref="string"/> uses text,
    /// <see cref="byte"/> arrays use raw bytes, and <see cref="Texture2D"/> uses a texture handler.
    /// Prefer per-request or per-endpoint non-JSON overrides instead of changing
    /// ApiClientConfig.DefaultResponseFormat for an entire client.
    /// </summary>
    public enum ApiResponseFormat
    {
        /// <summary>Infer the response format from the generic response type.</summary>
        Auto = 0,

        /// <summary>Deserialize the response body as JSON.</summary>
        Json = 1,

        /// <summary>Return the response body as text. Valid with <see cref="string"/> responses.</summary>
        Text = 2,

        /// <summary>Return the response body as raw bytes. Valid with <see cref="byte"/> array responses.</summary>
        Bytes = 3,

        /// <summary>Decode the response body as a texture. Valid with <see cref="Texture2D"/> responses.</summary>
        Texture = 4
    }
}
