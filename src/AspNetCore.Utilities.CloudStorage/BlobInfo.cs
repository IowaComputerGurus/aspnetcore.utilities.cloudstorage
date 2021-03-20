using System;

namespace ICG.AspNetCore.Utilities.CloudStorage
{
    /// <summary>
    ///     Stores basic information about blobs
    /// </summary>
    public class BlobInfo
    {
        /// <summary>
        ///     The name of the blob
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        ///     The size of the blob, if known.
        /// </summary>
        public long? ObjectSize { get; set; }

        /// <summary>
        ///     The download path to the blob, with provided configuration enabled
        /// </summary>
        public string DownloadPath { get; set; }

        /// <summary>
        ///     The content type defined for the blob
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        ///     The date the blob was created
        /// </summary>
        public DateTimeOffset? CreatedDate { get; set; }

        /// <summary>
        ///     The date the blog was edited
        /// </summary>
        public DateTimeOffset? LastEditedDate { get; set; }

        /// <summary>
        ///     The date the blog was last accessed
        /// </summary>
        public DateTimeOffset? LastAccessDate { get; set; }
    }
}