using System.ComponentModel.DataAnnotations;

namespace ICG.AspNetCore.Utilities.CloudStorage
{
    /// <summary>
    ///     Configuration options for the use of Azure Cloud Storage
    /// </summary>
    public class AzureCloudStorageOptions
    {
        /// <summary>
        ///     The storage account name to be used
        /// </summary>
        [Display(Name = "Storage Account")]
        public string StorageAccountName { get; set; }

        /// <summary>
        ///     An access key to use the storage account
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        ///     The root client path for the storage endpoint.  Either the CDN path, or storage account path
        /// </summary>
        [Display(Name = "Base URL")]
        public string RootClientPath { get; set; }
    }
}