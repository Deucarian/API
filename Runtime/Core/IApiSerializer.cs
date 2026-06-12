using System;
using Newtonsoft.Json;

namespace Deucarian.API.Core
{
    internal interface IApiSerializer
    {
        JsonSerializerSettings Settings { get; }
        string Serialize(object value);
        T Deserialize<T>(string json);
        object Deserialize(string json, Type type);
    }
}
