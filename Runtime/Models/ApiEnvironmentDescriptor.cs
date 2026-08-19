using System;
using System.Collections.Generic;

namespace Deucarian.API.Models
{
    /// <summary>Vendor-neutral lifecycle stage for an API deployment.</summary>
    public enum ApiEnvironmentStage
    {
        /// <summary>No standard deployment stage has been assigned.</summary>
        Custom = 0,

        /// <summary>Developer-facing integration environment.</summary>
        Development = 1,

        /// <summary>Automated or manual test environment.</summary>
        Testing = 2,

        /// <summary>User-acceptance environment.</summary>
        Acceptance = 3,

        /// <summary>Live production environment.</summary>
        Production = 4
    }

    /// <summary>Shared ordering for the four conventional deployment stages.</summary>
    public static class ApiEnvironmentStages
    {
        private static readonly IReadOnlyList<ApiEnvironmentStage> standard =
            Array.AsReadOnly(new[]
            {
                ApiEnvironmentStage.Development,
                ApiEnvironmentStage.Testing,
                ApiEnvironmentStage.Acceptance,
                ApiEnvironmentStage.Production
            });

        /// <summary>Development, Testing, Acceptance, and Production in order.</summary>
        public static IReadOnlyList<ApiEnvironmentStage> Standard => standard;
    }

    /// <summary>
    /// Safe metadata for a known environment, including environments that do
    /// not yet have a configured host or client profile.
    /// </summary>
    public sealed class ApiEnvironmentDescriptor
    {
        public ApiEnvironmentDescriptor(
            ApiEnvironmentId environmentId,
            ApiEnvironmentStage stage,
            string displayName)
        {
            if (environmentId.IsEmpty)
            {
                throw new ArgumentException(
                    "A known API environment requires a stable ID.",
                    nameof(environmentId));
            }

            if ((int)stage < (int)ApiEnvironmentStage.Custom ||
                (int)stage > (int)ApiEnvironmentStage.Production)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stage),
                    "The API environment stage is not supported.");
            }

            EnvironmentId = environmentId;
            Stage = stage;
            DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? environmentId.Value
                : displayName.Trim();
        }

        /// <summary>Stable vendor- or product-owned environment ID.</summary>
        public ApiEnvironmentId EnvironmentId { get; }

        /// <summary>Vendor-neutral deployment stage.</summary>
        public ApiEnvironmentStage Stage { get; }

        /// <summary>Safe human-friendly label that must not contain connection details.</summary>
        public string DisplayName { get; }
    }
}
