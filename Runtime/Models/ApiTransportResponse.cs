using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Deucarian.API.Models
{
    internal sealed class ApiTransportResponse
    {
        public long StatusCode { get; set; }

        public string RequestUrl { get; set; }

        public string RawBody { get; set; }

        public byte[] RawBytes { get; set; }

        public Texture2D Texture { get; set; }

        public string TransportError { get; set; }

        public UnityWebRequest.Result UnityResult { get; set; }

        public Dictionary<string, string> ResponseHeaders { get; set; } =
                new Dictionary<string, string>();

        public bool IsSuccessStatusCode => StatusCode >= 200 && StatusCode < 300;
    }
}
