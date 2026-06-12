using System;
using Deucarian.API.Configuration;
using Newtonsoft.Json;

namespace Deucarian.API.Core
{
    internal sealed class NewtonsoftApiSerializer : IApiSerializer
    {
        public NewtonsoftApiSerializer(ApiJsonSerializerOptions options = null)
        {
            Settings = (options ?? new ApiJsonSerializerOptions()).CreateSettings();
        }

        public NewtonsoftApiSerializer(JsonSerializerSettings settings)
        {
            Settings = settings ?? new ApiJsonSerializerOptions().CreateSettings();
        }

        public JsonSerializerSettings Settings { get; }

        public string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, Settings);
        }

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }

        public object Deserialize(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type, Settings);
        }
    }
}
