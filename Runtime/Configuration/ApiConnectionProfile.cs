using System;
using System.Collections.Generic;
using Deucarian.API.Core;
using Deucarian.API.Models;
using UnityEngine;

namespace Deucarian.API.Configuration
{
    /// <summary>
    /// Serializable form of safe known-environment metadata. It contains no
    /// host, header, credential, or active-environment state.
    /// </summary>
    [Serializable]
    public sealed class ApiEnvironmentDescriptorDefinition
    {
        [Tooltip("Stable environment identifier, for example 'development' or 'vendor.acceptance'.")]
        [SerializeField] private string environmentId;

        [Tooltip("Vendor-neutral lifecycle stage used for ordering and status UI.")]
        [SerializeField] private ApiEnvironmentStage stage;

        [Tooltip("Safe human-friendly label. Do not include a host or credential.")]
        [SerializeField] private string displayName;

        /// <summary>Stable environment identifier.</summary>
        public string EnvironmentId
        {
            get => environmentId;
            set => environmentId = value;
        }

        /// <summary>Vendor-neutral lifecycle stage.</summary>
        public ApiEnvironmentStage Stage
        {
            get => stage;
            set => stage = value;
        }

        /// <summary>Safe human-friendly label.</summary>
        public string DisplayName
        {
            get => displayName;
            set => displayName = value;
        }

        /// <summary>Creates validated runtime descriptor metadata.</summary>
        public bool TryCreateDescriptor(
            out ApiEnvironmentDescriptor descriptor,
            out string message)
        {
            descriptor = null;
            if (!ApiEnvironmentId.TryParse(environmentId, out ApiEnvironmentId id))
            {
                message = "Known environment has an invalid stable ID: '" +
                          (environmentId ?? string.Empty) + "'.";
                return false;
            }

            try
            {
                descriptor = new ApiEnvironmentDescriptor(id, stage, displayName);
                message = null;
                return true;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                message = exception.Message;
                return false;
            }
        }

        /// <summary>Creates a serializable definition from runtime metadata.</summary>
        public static ApiEnvironmentDescriptorDefinition FromDescriptor(
            ApiEnvironmentDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            return new ApiEnvironmentDescriptorDefinition
            {
                EnvironmentId = descriptor.EnvironmentId.Value,
                Stage = descriptor.Stage,
                DisplayName = descriptor.DisplayName
            };
        }
    }

    /// <summary>
    /// Project-facing API connection contract. It combines environment-specific
    /// hosts with one host-free endpoint catalog without storing credentials or
    /// an active environment.
    /// </summary>
    public sealed class ApiConnectionProfile : ScriptableObject
    {
        [Tooltip("Environment profiles containing named clients and project-owned base URLs.")]
        [SerializeField] private List<ApiEnvironmentProfile> environments =
            new List<ApiEnvironmentProfile>();

        [Tooltip("Endpoint contract containing stable IDs, relative routes, methods, and authentication rules.")]
        [SerializeField] private ApiEndpointCatalog endpointCatalog;

        [Tooltip("Safe metadata for known environments, including slots whose hosts are not configured yet.")]
        [SerializeField] private List<ApiEnvironmentDescriptorDefinition>
            knownEnvironmentDefinitions =
                new List<ApiEnvironmentDescriptorDefinition>();

        /// <summary>Environment-specific host profiles in this connection.</summary>
        public IReadOnlyList<ApiEnvironmentProfile> Environments => environments;

        /// <summary>Endpoint contract used by this connection.</summary>
        public ApiEndpointCatalog EndpointCatalog
        {
            get => endpointCatalog;
            set => endpointCatalog = value;
        }

        /// <summary>Serializable safe metadata for known environments.</summary>
        public IReadOnlyList<ApiEnvironmentDescriptorDefinition>
            KnownEnvironmentDefinitions => knownEnvironmentDefinitions;

        /// <summary>
        /// Creates a validated composition. Blank environment slots remain
        /// known but cannot resolve traffic.
        /// </summary>
        public ApiComposition CreateComposition()
        {
            if (endpointCatalog == null)
            {
                throw new InvalidOperationException(
                    "Assign an endpoint catalog before creating an API composition.");
            }

            List<ApiEnvironmentDescriptor> descriptors =
                CreateKnownEnvironmentDescriptors();
            return new ApiComposition(
                environments,
                endpointCatalog,
                descriptors);
        }

        /// <summary>Attempts to create a validated composition with a developer-facing error.</summary>
        public bool TryCreateComposition(
            out ApiComposition composition,
            out string message)
        {
            try
            {
                composition = CreateComposition();
                message = null;
                return true;
            }
            catch (ArgumentException exception)
            {
                composition = null;
                message = exception.Message;
                return false;
            }
            catch (InvalidOperationException exception)
            {
                composition = null;
                message = exception.Message;
                return false;
            }
        }

        /// <summary>
        /// Creates an unsaved profile for integration packages, tests, and
        /// editor factories.
        /// </summary>
        public static ApiConnectionProfile CreateTransient(
            IEnumerable<ApiEnvironmentProfile> environmentProfiles,
            ApiEndpointCatalog catalog,
            IEnumerable<ApiEnvironmentDescriptor> knownDescriptors = null)
        {
            ApiConnectionProfile profile = CreateInstance<ApiConnectionProfile>();
            profile.environments.Clear();
            if (environmentProfiles != null)
            {
                profile.environments.AddRange(environmentProfiles);
            }

            profile.endpointCatalog = catalog;
            profile.knownEnvironmentDefinitions.Clear();
            if (knownDescriptors != null)
            {
                foreach (ApiEnvironmentDescriptor descriptor in knownDescriptors)
                {
                    profile.knownEnvironmentDefinitions.Add(
                        ApiEnvironmentDescriptorDefinition.FromDescriptor(descriptor));
                }
            }

            return profile;
        }

        /// <summary>Returns validated runtime descriptors for status and composition UI.</summary>
        public bool TryGetKnownEnvironmentDescriptors(
            out IReadOnlyList<ApiEnvironmentDescriptor> descriptors,
            out string message)
        {
            try
            {
                descriptors = CreateKnownEnvironmentDescriptors();
                message = null;
                return true;
            }
            catch (ArgumentException exception)
            {
                descriptors = null;
                message = exception.Message;
                return false;
            }
            catch (InvalidOperationException exception)
            {
                descriptors = null;
                message = exception.Message;
                return false;
            }
        }

        private List<ApiEnvironmentDescriptor> CreateKnownEnvironmentDescriptors()
        {
            var descriptors = new List<ApiEnvironmentDescriptor>();
            if (knownEnvironmentDefinitions == null)
            {
                return descriptors;
            }

            var ids = new HashSet<ApiEnvironmentId>();
            foreach (ApiEnvironmentDescriptorDefinition definition in
                     knownEnvironmentDefinitions)
            {
                if (definition == null)
                {
                    throw new InvalidOperationException(
                        "Known environment metadata contains a null entry.");
                }

                if (!definition.TryCreateDescriptor(
                        out ApiEnvironmentDescriptor descriptor,
                        out string message))
                {
                    throw new InvalidOperationException(message);
                }

                if (!ids.Add(descriptor.EnvironmentId))
                {
                    throw new InvalidOperationException(
                        "Duplicate known environment ID '" +
                        descriptor.EnvironmentId + "'.");
                }

                descriptors.Add(descriptor);
            }

            return descriptors;
        }
    }
}
