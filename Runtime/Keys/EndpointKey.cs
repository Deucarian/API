using System;
using UnityEngine;

namespace Deucarian.API
{
    /// <summary>A declared Endpoint identity. Reuse a named definition or select it in the Inspector.</summary>
    [Serializable]
    public class EndpointKey<TRequest, TResponse> : IEquatable<EndpointKey<TRequest, TResponse>>
    {
        [SerializeField] private string definitionId;

        /// <summary>For central definition sets and generated declarations; ordinary callers reuse those keys.</summary>
        protected EndpointKey(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id != id.Trim())
                throw new ArgumentException("A EndpointKey definition needs a non-empty stable ID without surrounding whitespace.", nameof(id));
            definitionId = id;
        }

        public string Id => !string.IsNullOrWhiteSpace(definitionId) ? definitionId :
            throw new InvalidOperationException("No EndpointKey is selected. Select an existing definition in the Inspector or assign a named key from a EndpointKeySet declaration.");
        public bool Equals(EndpointKey<TRequest, TResponse> other) => other != null && string.Equals(definitionId, other.definitionId, StringComparison.Ordinal);
        public override bool Equals(object other) => other is EndpointKey<TRequest, TResponse> key && Equals(key);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(definitionId ?? string.Empty);
        public override string ToString() => definitionId ?? string.Empty;
    }
}
