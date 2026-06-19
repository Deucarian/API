namespace Deucarian.API.Models
{
    /// <summary>
    /// Controls how AssetBundle responses are requested through UnityWebRequestAssetBundle.
    /// </summary>
    public enum ApiAssetBundleCacheMode
    {
        /// <summary>Use Unity's default AssetBundle request behavior unless cache metadata is supplied.</summary>
        Default = 0,

        /// <summary>Do not opt into Unity's version/hash AssetBundle cache overloads.</summary>
        Disabled = 1,

        /// <summary>Use Unity's AssetBundle cache when cache hash or version metadata is supplied.</summary>
        UseUnityCache = 2
    }

    /// <summary>
    /// Optional request metadata for <see cref="ApiResponseFormat.AssetBundle"/> calls.
    /// </summary>
    public sealed class ApiAssetBundleRequestOptions
    {
        /// <summary>CRC passed to Unity's AssetBundle request overloads when supported.</summary>
        public uint Crc { get; set; }

        /// <summary>Cache behavior for Unity's AssetBundle request overloads.</summary>
        public ApiAssetBundleCacheMode CacheMode { get; set; } = ApiAssetBundleCacheMode.Default;

        /// <summary>Optional cache key used together with <see cref="CacheHash"/>.</summary>
        public string CacheKey { get; set; }

        /// <summary>Optional Unity Hash128 string used for hash-based AssetBundle caching.</summary>
        public string CacheHash { get; set; }

        /// <summary>Optional version used for version-based AssetBundle caching.</summary>
        public uint? CacheVersion { get; set; }
    }
}
