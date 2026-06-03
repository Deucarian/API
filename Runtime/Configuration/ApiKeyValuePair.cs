using System;
using UnityEngine;

namespace JorisHoef.APIHelper.Configuration
{
    /// <summary>
    /// Serializable key/value pair used for headers and query parameters in ScriptableObject assets.
    /// </summary>
    [Serializable]
    public sealed class ApiKeyValuePair
    {
        [SerializeField] private string key;
        [SerializeField] private string value;

        /// <summary>Header or query parameter key.</summary>
        public string Key
        {
            get => key;
            set => key = value;
        }

        /// <summary>Header or query parameter value.</summary>
        public string Value
        {
            get => value;
            set => this.value = value;
        }
    }
}
