using System;
using System.Collections;
using System.IO;
using System.Threading;
using Deucarian.API.Core;
using Deucarian.API.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace Deucarian.API.Tests
{
    public sealed class ApiMediaResponseAllocationTests
    {
        [UnityTest]
        public IEnumerator TextureResponseDoesNotRetainAnEncodedManagedCopy()
        {
            string file = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
            var source = new Texture2D(32, 32, TextureFormat.RGB24, false);
            Texture2D loaded = null;
            try
            {
                File.WriteAllBytes(file, source.EncodeToPNG());
                using (var request = UnityWebRequestTexture.GetTexture(new Uri(file).AbsoluteUri, true))
                {
                    var task = new UnityWebRequestSender().SendAsync(request, new ApiRequest(request.url),
                        ApiResponseFormat.Texture, CancellationToken.None);
                    while (!task.IsCompleted) yield return null;
                    var result = task.GetAwaiter().GetResult();
                    loaded = result.Texture;
                    Assert.IsNotNull(loaded);
                    Assert.AreEqual(32, loaded.width);
                    Assert.IsNull(result.RawBytes, "Texture downloads must not allocate a redundant byte array.");
                    Assert.IsNull(result.RawBody, "Binary responses must not be decoded to text.");
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(source); UnityEngine.Object.DestroyImmediate(loaded); File.Delete(file); }
        }

        [UnityTest]
        public IEnumerator ByteResponseRetainsBytesWithoutCreatingATextCopy()
        {
            string file = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bin");
            byte[] data = { 255, 0, 254, 128, 10, 17 };
            try
            {
                File.WriteAllBytes(file, data);
                using (var request = UnityWebRequest.Get(new Uri(file).AbsoluteUri))
                {
                    var task = new UnityWebRequestSender().SendAsync(request, new ApiRequest(request.url),
                        ApiResponseFormat.Bytes, CancellationToken.None);
                    while (!task.IsCompleted) yield return null;
                    var result = task.GetAwaiter().GetResult();
                    CollectionAssert.AreEqual(data, result.RawBytes);
                    Assert.IsNull(result.RawBody);
                }
            }
            finally { File.Delete(file); }
        }

        [Test]
        public void MissingTextureFailsEvenWithoutAnEncodedCopy()
        {
            var parser = new ApiResponseParser(new NewtonsoftApiSerializer());
            Assert.Throws<InvalidOperationException>(() => parser.Parse<Texture2D>(
                new ApiRequest("image"), new ApiTransportResponse(), ApiResponseFormat.Texture));
        }
    }
}
