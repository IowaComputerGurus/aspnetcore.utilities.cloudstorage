namespace ICG.AspNetCore.Utilities.CloudStorage
{
    /// <summary>
    ///     Representation of a
    /// </summary>
    public class BlobUriInfo
    {
        /// <summary>
        ///     The raw URL, typically blob or CDN
        /// </summary>
        public string RootUrl { get; set; }

        /// <summary>
        ///     The container
        /// </summary>
        public string Container { get; set; }

        /// <summary>
        ///     The name of the blob
        /// </summary>
        public string BlobName { get; set; }
    }
}