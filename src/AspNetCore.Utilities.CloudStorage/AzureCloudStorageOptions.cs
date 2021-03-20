using System.ComponentModel.DataAnnotations;

namespace ICG.AspNetCore.Utilities.CloudStorage
{
    /// <summary>
    ///     Configuration options for the use of Azure Cloud Storage
    /// </summary>
    public class AzureCloudStorageOptions
    {
        /// <summary>
        ///     The connection string to the blob storage account
        /// </summary>
        [Display(Name = "Connection String")]
        public string StorageConnectionString { get; set; }

        /// <summary>
        ///     The root client path for the storage endpoint.  Either the CDN path, or storage account path
        /// </summary>
        [Display(Name = "Base URL")]
        public string RootClientPath { get; set; }
    }
}