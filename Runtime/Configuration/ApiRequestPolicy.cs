using System;
using UnityEngine;

namespace Deucarian.API.Configuration
{
    /// <summary>
    /// Resolved request policy values. The built-in client applies <see cref="TimeoutSeconds"/>;
    /// retry, backoff, and rate-limit values are explicit metadata for policy-aware callers and decorators.
    /// </summary>
    public sealed class ApiRequestPolicy
    {
        /// <summary>The safe package default.</summary>
        public static ApiRequestPolicy Default { get; } =
                new ApiRequestPolicy(30, 0, 250, 2f, 5000, 0, 0f);

        /// <summary>Creates a complete, validated request policy.</summary>
        public ApiRequestPolicy(int timeoutSeconds,
                                int maxRetryAttempts,
                                int initialRetryBackoffMilliseconds,
                                float retryBackoffMultiplier,
                                int maximumRetryBackoffMilliseconds,
                                int rateLimitRequestCountHint,
                                float rateLimitWindowSecondsHint)
        {
            if (timeoutSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeoutSeconds));
            }

            if (maxRetryAttempts < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRetryAttempts));
            }

            if (initialRetryBackoffMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialRetryBackoffMilliseconds));
            }

            if (retryBackoffMultiplier < 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(retryBackoffMultiplier));
            }

            if (maximumRetryBackoffMilliseconds < initialRetryBackoffMilliseconds)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumRetryBackoffMilliseconds));
            }

            if (rateLimitRequestCountHint < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rateLimitRequestCountHint));
            }

            if (rateLimitWindowSecondsHint < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(rateLimitWindowSecondsHint));
            }

            TimeoutSeconds = timeoutSeconds;
            MaxRetryAttempts = maxRetryAttempts;
            InitialRetryBackoffMilliseconds = initialRetryBackoffMilliseconds;
            RetryBackoffMultiplier = retryBackoffMultiplier;
            MaximumRetryBackoffMilliseconds = maximumRetryBackoffMilliseconds;
            RateLimitRequestCountHint = rateLimitRequestCountHint;
            RateLimitWindowSecondsHint = rateLimitWindowSecondsHint;
        }

        /// <summary>Request timeout. Zero leaves timeout handling to UnityWebRequest.</summary>
        public int TimeoutSeconds { get; }

        /// <summary>Maximum additional attempts requested by a policy-aware caller. Zero disables retries.</summary>
        public int MaxRetryAttempts { get; }

        /// <summary>Initial delay before a retry, in milliseconds.</summary>
        public int InitialRetryBackoffMilliseconds { get; }

        /// <summary>Multiplier used to increase successive retry delays.</summary>
        public float RetryBackoffMultiplier { get; }

        /// <summary>Upper bound for a calculated retry delay, in milliseconds.</summary>
        public int MaximumRetryBackoffMilliseconds { get; }

        /// <summary>Advisory request count for a rate-limit window. Zero means unspecified.</summary>
        public int RateLimitRequestCountHint { get; }

        /// <summary>Advisory rate-limit window in seconds. Zero means unspecified.</summary>
        public float RateLimitWindowSecondsHint { get; }

        /// <summary>Calculates a bounded backoff delay for a one-based retry attempt.</summary>
        public int GetRetryBackoffMilliseconds(int retryAttempt)
        {
            if (retryAttempt < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(retryAttempt));
            }

            double delay = InitialRetryBackoffMilliseconds
                           * Math.Pow(RetryBackoffMultiplier, retryAttempt - 1);
            return (int)Math.Min(MaximumRetryBackoffMilliseconds, delay);
        }
    }

    /// <summary>
    /// Serializable policy overlay. A value of -1 inherits from the previous composition level;
    /// the multiplier uses 0 to inherit because valid multipliers start at 1.
    /// </summary>
    [Serializable]
    public sealed class ApiRequestPolicyDefinition
    {
        [Tooltip("Request timeout in seconds. -1 inherits, 0 leaves timeout handling to UnityWebRequest.")]
        [SerializeField] private int timeoutSeconds = -1;

        [Tooltip("Maximum additional attempts for a policy-aware caller. -1 inherits.")]
        [SerializeField] private int maxRetryAttempts = -1;

        [Tooltip("Initial retry delay in milliseconds. -1 inherits.")]
        [SerializeField] private int initialRetryBackoffMilliseconds = -1;

        [Tooltip("Retry backoff multiplier. 0 inherits; configured values must be at least 1.")]
        [SerializeField] private float retryBackoffMultiplier;

        [Tooltip("Maximum retry delay in milliseconds. -1 inherits.")]
        [SerializeField] private int maximumRetryBackoffMilliseconds = -1;

        [Tooltip("Advisory requests per rate-limit window. -1 inherits; 0 means unspecified.")]
        [SerializeField] private int rateLimitRequestCountHint = -1;

        [Tooltip("Advisory rate-limit window in seconds. -1 inherits; 0 means unspecified.")]
        [SerializeField] private float rateLimitWindowSecondsHint = -1f;

        /// <summary>Timeout override in seconds, or -1 to inherit.</summary>
        public int TimeoutSeconds { get => timeoutSeconds; set => timeoutSeconds = value; }

        /// <summary>Maximum additional attempts, or -1 to inherit.</summary>
        public int MaxRetryAttempts { get => maxRetryAttempts; set => maxRetryAttempts = value; }

        /// <summary>Initial retry delay in milliseconds, or -1 to inherit.</summary>
        public int InitialRetryBackoffMilliseconds
        {
            get => initialRetryBackoffMilliseconds;
            set => initialRetryBackoffMilliseconds = value;
        }

        /// <summary>Retry delay multiplier, or 0 to inherit.</summary>
        public float RetryBackoffMultiplier
        {
            get => retryBackoffMultiplier;
            set => retryBackoffMultiplier = value;
        }

        /// <summary>Maximum retry delay in milliseconds, or -1 to inherit.</summary>
        public int MaximumRetryBackoffMilliseconds
        {
            get => maximumRetryBackoffMilliseconds;
            set => maximumRetryBackoffMilliseconds = value;
        }

        /// <summary>Advisory requests per rate-limit window, or -1 to inherit.</summary>
        public int RateLimitRequestCountHint
        {
            get => rateLimitRequestCountHint;
            set => rateLimitRequestCountHint = value;
        }

        /// <summary>Advisory rate-limit window seconds, or -1 to inherit.</summary>
        public float RateLimitWindowSecondsHint
        {
            get => rateLimitWindowSecondsHint;
            set => rateLimitWindowSecondsHint = value;
        }

        /// <summary>Resolves this overlay against an already resolved fallback policy.</summary>
        public ApiRequestPolicy Resolve(ApiRequestPolicy fallback = null)
        {
            fallback = fallback ?? ApiRequestPolicy.Default;
            string validationMessage;
            if (!IsValid(out validationMessage))
            {
                throw new InvalidOperationException(validationMessage);
            }

            int resolvedInitial = initialRetryBackoffMilliseconds >= 0
                                          ? initialRetryBackoffMilliseconds
                                          : fallback.InitialRetryBackoffMilliseconds;
            int resolvedMaximum = maximumRetryBackoffMilliseconds >= 0
                                          ? maximumRetryBackoffMilliseconds
                                          : fallback.MaximumRetryBackoffMilliseconds;
            if (resolvedMaximum < resolvedInitial)
            {
                throw new InvalidOperationException(
                    "Maximum retry backoff cannot be lower than the resolved initial retry backoff.");
            }

            return new ApiRequestPolicy(
                timeoutSeconds >= 0 ? timeoutSeconds : fallback.TimeoutSeconds,
                maxRetryAttempts >= 0 ? maxRetryAttempts : fallback.MaxRetryAttempts,
                resolvedInitial,
                retryBackoffMultiplier >= 1f ? retryBackoffMultiplier : fallback.RetryBackoffMultiplier,
                resolvedMaximum,
                rateLimitRequestCountHint >= 0
                        ? rateLimitRequestCountHint
                        : fallback.RateLimitRequestCountHint,
                rateLimitWindowSecondsHint >= 0f
                        ? rateLimitWindowSecondsHint
                        : fallback.RateLimitWindowSecondsHint);
        }

        /// <summary>Validates only values explicitly overridden by this definition.</summary>
        public bool IsValid(out string message)
        {
            if (timeoutSeconds < -1
                || maxRetryAttempts < -1
                || initialRetryBackoffMilliseconds < -1
                || maximumRetryBackoffMilliseconds < -1
                || rateLimitRequestCountHint < -1
                || rateLimitWindowSecondsHint < -1f)
            {
                message = "Policy values must be -1 to inherit or a non-negative value.";
                return false;
            }

            if (retryBackoffMultiplier < 0f
                || (retryBackoffMultiplier > 0f && retryBackoffMultiplier < 1f))
            {
                message = "Retry backoff multiplier must be 0 to inherit or at least 1.";
                return false;
            }

            if (initialRetryBackoffMilliseconds >= 0
                && maximumRetryBackoffMilliseconds >= 0
                && maximumRetryBackoffMilliseconds < initialRetryBackoffMilliseconds)
            {
                message = "Maximum retry backoff cannot be lower than initial retry backoff.";
                return false;
            }

            message = null;
            return true;
        }
    }
}
