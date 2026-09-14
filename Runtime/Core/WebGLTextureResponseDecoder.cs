#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.Common;
using UnityEngine;

namespace Deucarian.API.Core
{
    /// <summary>Browser decoding and bounded uploads; Unity retains ownership of the texture.</summary>
    internal static class WebGLTextureResponseDecoder
    {
        [DllImport("__Internal")] private static extern int DeucarianApiTextureBegin(byte[] bytes, int length);
        [DllImport("__Internal")] private static extern int DeucarianApiTextureState(int id);
        [DllImport("__Internal")] private static extern int DeucarianApiTextureWidth(int id);
        [DllImport("__Internal")] private static extern int DeucarianApiTextureHeight(int id);
        [DllImport("__Internal")] private static extern void DeucarianApiTextureUpload(int id, int texture);
        [DllImport("__Internal")] private static extern void DeucarianApiTextureRelease(int id);

        internal static async Task<Texture2D> DecodeAsync(byte[] bytes, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (bytes == null || bytes.Length == 0) throw new InvalidOperationException("Empty texture response.");
            int id = DeucarianApiTextureBegin(bytes, bytes.Length);
            Texture2D texture = null;
            bool transferred = false;
            try
            {
                // Discard the managed encoded copy while the browser owns its immutable Blob.
                bytes = null;
                float deadline = Time.realtimeSinceStartup + 60f;
                while (DeucarianApiTextureState(id) == 0)
                    await NextAsync(cancellationToken, deadline);
                if (DeucarianApiTextureState(id) < 0) throw new InvalidOperationException("Browser image decoding failed.");
                int width = DeucarianApiTextureWidth(id), height = DeucarianApiTextureHeight(id);
                if (width < 1 || height < 1 || width > SystemInfo.maxTextureSize || height > SystemInfo.maxTextureSize)
                    throw new InvalidOperationException("Image dimensions exceed the device texture limit.");
                await NextAsync(cancellationToken, deadline);
                texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false, true)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
                await NextAsync(cancellationToken, deadline);
                DeucarianApiTextureUpload(id, texture.GetNativeTexturePtr().ToInt32());
                while (DeucarianApiTextureState(id) == 2)
                    await NextAsync(cancellationToken, deadline);
                cancellationToken.ThrowIfCancellationRequested();
                if (DeucarianApiTextureState(id) != 3) throw new InvalidOperationException("Browser texture upload failed.");
                transferred = true;
                return texture;
            }
            finally
            {
                DeucarianApiTextureRelease(id);
                if (!transferred) UnityObjectUtility.DestroySafely(texture);
            }
        }

        private static async Task NextAsync(CancellationToken token, float deadline)
        {
            token.ThrowIfCancellationRequested();
            if (Time.realtimeSinceStartup > deadline) throw new TimeoutException("Browser image processing timed out.");
            await Task.Yield();
            token.ThrowIfCancellationRequested();
        }
    }
}
#endif
