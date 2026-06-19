namespace Deucarian.API.Models
{
    /// <summary>
    /// Transport progress reported while an API request is in flight.
    /// </summary>
    public sealed class ApiTransferProgress
    {
        private ApiTransferProgress()
        {
        }

        /// <summary>Normalized download progress in the range 0..1 when Unity can report it.</summary>
        public float DownloadProgress { get; private set; }

        /// <summary>Normalized upload progress in the range 0..1 when Unity can report it.</summary>
        public float UploadProgress { get; private set; }

        /// <summary>Total downloaded byte count reported by Unity.</summary>
        public long DownloadedBytes { get; private set; }

        /// <summary>Total uploaded byte count reported by Unity.</summary>
        public long UploadedBytes { get; private set; }

        /// <summary>True for the final progress notification after the transfer completes.</summary>
        public bool IsDone { get; private set; }

        public static ApiTransferProgress Create(float downloadProgress,
                                                 float uploadProgress,
                                                 long downloadedBytes,
                                                 long uploadedBytes,
                                                 bool isDone)
        {
            return new ApiTransferProgress
            {
                DownloadProgress = Clamp01(downloadProgress),
                UploadProgress = Clamp01(uploadProgress),
                DownloadedBytes = downloadedBytes < 0 ? 0 : downloadedBytes,
                UploadedBytes = uploadedBytes < 0 ? 0 : uploadedBytes,
                IsDone = isDone
            };
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            return value > 1f ? 1f : value;
        }
    }
}
