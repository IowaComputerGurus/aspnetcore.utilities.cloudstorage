using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace ICG.AspNetCore.Utilities.CloudStorage
{
    /// <summary>
    ///     Provides a streamlined interface for storing objects to Azure Storage
    /// </summary>
    public interface IAzureCloudStorageProvider
    {
        /// <summary>
        ///     Stores an object to AzureStorage.  This implementation will set the mime-time based on the requested name
        /// </summary>
        /// <param name="desiredContainer">The desired container within Azure Storage to place the file</param>
        /// <param name="fileContents">A stream containing the file contents</param>
        /// <param name="desiredName">The desired name/path of the object inside of the container</param>
        /// <returns>The path to the loaded file</returns>
        Task<string> StoreObject(string desiredContainer, Stream fileContents, string desiredName);

        /// <summary>
        ///     Stores an object into Cloud Storage from a provided array of bytes
        /// </summary>
        /// <param name="desiredContainer">The desired target container</param>
        /// <param name="fileContents">The file content to load</param>
        /// <param name="desiredName">The desired name for final storage, including any needed path or extension</param>
        /// <returns>The URL to the blob as stored.</returns>
        Task<string> StoreObject(string desiredContainer, byte[] fileContents, string desiredName);

        /// <summary>
        ///     Removes an object from Azure Storage
        /// </summary>
        /// <param name="expectedContainer">The container containing the object</param>
        /// <param name="fullObjectUrl">The full URL to the object to be deleted</param>
        /// <returns>[True] if the object was deleted [False] if it was not</returns>
        Task<bool> DeleteObject(string expectedContainer, string fullObjectUrl);

        /// <summary>
        ///     Gets the name of the object
        /// </summary>
        /// <param name="container">The container the object should be found in</param>
        /// <param name="fullObjectPath">The full download path of the object</param>
        /// <returns>The name inside of the container of the object.  Null if the container couldn't be located in path</returns>
        string GetObjectName(string container, string fullObjectPath);

        /// <summary>
        ///     Uploads the results of an IFormFile to Azure Storage, using a string input to create a "slug" of content
        /// </summary>
        /// <example>
        ///     Uploading with slugContent = "My Test Slug" a file named "text.pdf" will result in the file uploading as
        ///     my-test-slug.pdf.  Taking the slug and adding the proper file extension.
        /// </example>
        /// <param name="file">The posted file to upload</param>
        /// <param name="container">The target container</param>
        /// <param name="slugContent">The desired slug content</param>
        /// <returns>The path to the uploaded image</returns>
        /// <exception cref="ArgumentNullException">If [file] is null</exception>
        /// <exception cref="FileLoadException">If an error occured uploading to Azure</exception>
        Task<string> UploadIFormFileWithSlug(IFormFile file, string container, string slugContent);
    }

    /// <inheritdoc />
    public class AzureCloudStorageProvider : IAzureCloudStorageProvider
    {
        private readonly IMimeTypeMapper _mimeTypeMapper;
        private readonly IOptions<AzureCloudStorageOptions> _storageOptions;
        private readonly IUrlSlugGenerator _urlSlugGenerator;

        /// <summary>
        ///     Default constructor with proper dependencies injected
        /// </summary>
        /// <param name="storageOptions">The configuration options</param>
        /// <param name="urlSlugGenerator">The URL Slug Generator</param>
        /// <param name="mimeTypeMapper">Mime-Type mapper for proper storage</param>
        public AzureCloudStorageProvider(IOptions<AzureCloudStorageOptions> storageOptions,
            IUrlSlugGenerator urlSlugGenerator, IMimeTypeMapper mimeTypeMapper)
        {
            _storageOptions = storageOptions;
            _urlSlugGenerator = urlSlugGenerator;
            _mimeTypeMapper = mimeTypeMapper;
        }

        /// <inheritdoc />
        public async Task<string> StoreObject(string desiredContainer, Stream fileContents, string desiredName)
        {
            //Get the storage account
            var storageAccount =
                new CloudStorageAccount(
                    new StorageCredentials(_storageOptions.Value.StorageAccountName, _storageOptions.Value.AccessKey),
                    true);

            //Create the client & container reference
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(desiredContainer.ToLower());

            //Get our block/blob reference
            var blockBlob = container.GetBlockBlobReference(desiredName);

            //Delete existing if needed
            await blockBlob.DeleteIfExistsAsync();

            //Set content type if needed
            if (_mimeTypeMapper.TryGetMimeType(desiredName, out var contentType))
                blockBlob.Properties.ContentType = contentType;

            //Upload it
            await blockBlob.UploadFromStreamAsync(fileContents);

            return $"{_storageOptions.Value.RootClientPath}/{desiredContainer.ToLower()}/{desiredName}";
        }

        /// <summary>
        ///     Stores an object into Cloud Storage from a provided array of bytes
        /// </summary>
        /// <param name="desiredContainer">The desired target container</param>
        /// <param name="fileContents">The file content to load</param>
        /// <param name="desiredName">The desired name for final storage, including any needed path or extension</param>
        /// <returns>The URL to the blob as stored.</returns>
        public async Task<string> StoreObject(string desiredContainer, byte[] fileContents, string desiredName)
        {
            //Get the storage account
            var storageAccount =
                new CloudStorageAccount(
                    new StorageCredentials(_storageOptions.Value.StorageAccountName, _storageOptions.Value.AccessKey),
                    true);

            //Create the client & container reference
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(desiredContainer.ToLower());

            //Get our block/blob reference
            var blockBlob = container.GetBlockBlobReference(desiredName);

            //Delete existing if needed
            await blockBlob.DeleteIfExistsAsync();

            //Set content type if needed
            if (_mimeTypeMapper.TryGetMimeType(desiredName, out var contentType))
                blockBlob.Properties.ContentType = contentType;

            //Upload it
            await blockBlob.UploadFromByteArrayAsync(fileContents, 0, fileContents.Length);

            return $"{_storageOptions.Value.RootClientPath}/{desiredContainer.ToLower()}/{desiredName}";
        }

        /// <inheritdoc />
        public async Task<bool> DeleteObject(string expectedContainer, string fullObjectUrl)
        {
            //Get object name
            var objectName = GetObjectName(expectedContainer, fullObjectUrl);
            if (objectName == null)
                return false;

            //Get the storage account
            var storageAccount =
                new CloudStorageAccount(
                    new StorageCredentials(_storageOptions.Value.StorageAccountName, _storageOptions.Value.AccessKey),
                    true);

            //Create the client & container reference
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(expectedContainer.ToLower());

            //Get our block/blob reference
            var blockBlob = container.GetBlockBlobReference(objectName);

            //Delete
            return await blockBlob.DeleteIfExistsAsync();
        }

        /// <inheritdoc />
        public string GetObjectName(string container, string fullObjectPath)
        {
            //Get the container portion of the url
            var containerName = container.ToLower() + "/";
            if (fullObjectPath.IndexOf(containerName, StringComparison.OrdinalIgnoreCase) <= 0)
                return null;

            var pathWithContainer =
                fullObjectPath.Substring(fullObjectPath.IndexOf(containerName, StringComparison.OrdinalIgnoreCase));
            return pathWithContainer.Replace(containerName, string.Empty);
        }

        /// <inheritdoc />
        public async Task<string> UploadIFormFileWithSlug(IFormFile file, string container, string slugContent)
        {
            //Throw exception if no file
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            //Upload the file
            var urlSlug = _urlSlugGenerator.GenerateSlug(slugContent);
            string cdnPath;
            try
            {
                var targetFileName = $"{urlSlug}{Path.GetExtension(file.FileName)}";
                cdnPath = await StoreObject(container, file.OpenReadStream(), targetFileName);
            }
            catch (Exception ex)
            {
                throw new FileLoadException($"Error Uploading to Azure: {ex.Message}");
            }

            return cdnPath;
        }
    }
}