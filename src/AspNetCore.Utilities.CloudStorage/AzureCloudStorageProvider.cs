using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        ///     Taking an incoming full URL and decomposes it into a detailed object
        /// </summary>
        /// <param name="fullObjectPath">THe full URL to the object</param>
        /// <returns></returns>
        BlobUriInfo DecomposeBlobUrl(string fullObjectPath);

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

        /// <summary>
        ///     Uploads the results of an IFormFile to Azure Storage, using a specific file name for the destination file
        /// </summary>
        /// <param name="file">The posted file to upload</param>
        /// <param name="container">The target container</param>
        /// <param name="desiredName">The desired name of the file, including any path/extension</param>
        /// <returns>The path to the uploaded image</returns>
        /// <exception cref="ArgumentNullException">If [file] is null</exception>
        /// <exception cref="FileLoadException">If an error occured uploading to Azure</exception>
        Task<string> UploadIFormFile(IFormFile file, string container, string desiredName);

        /// <summary>
        ///     Provides a listing of all blobs in a requested container
        /// </summary>
        /// <remarks>
        ///     For very large containers this can take a long time to complete.
        /// </remarks>
        /// <param name="container"></param>
        /// <returns></returns>
        Task<List<BlobInfo>> ListBlobs(string container);

        /// <summary>
        ///     Creates a SAS token to access an object with the default duration
        /// </summary>
        /// <param name="fullObjectPath">The full path to the object</param>
        /// <returns>A URL with a SAS token</returns>
        string CreateSASUrl(string fullObjectPath);
        
        /// <summary>
        ///     Creates a SAS token to access an object with the specified duration
        /// </summary>
        /// <param name="fullObjectPath">The full path to the object</param>
        /// <param name="tokenDuration">The duration of token (in minutes)</param>
        /// <returns>A URL with a SAS token</returns>
        string CreateSASUrl(string fullObjectPath, int tokenDuration);

        /// <summary>
        ///     Creates a SAS token to access an object with the default duration
        /// </summary>
        /// <param name="container">The container</param>
        /// <param name="objectName">The objectName</param>
        /// <returns>A URL with a SAS token</returns>
        string CreateSASUrl(string container, string objectName);

        /// <summary>
        ///     Creates a SAS token to access an object with the default duration
        /// </summary>
        /// <param name="container">The container</param>
        /// <param name="objectName">The objectName</param>
        /// <param name="tokenDuration">The duration of token (in minutes)</param>
        /// <returns>A URL with a SAS token</returns>
        string CreateSASUrl(string container, string objectName, int tokenDuration);
    }

    /// <inheritdoc />
    public class AzureCloudStorageProvider : IAzureCloudStorageProvider
    {
        private readonly ILogger _logger;
        private readonly IMimeTypeMapper _mimeTypeMapper;
        private readonly IOptions<AzureCloudStorageOptions> _storageOptions;
        private readonly IUrlSlugGenerator _urlSlugGenerator;

        /// <summary>
        ///     Default constructor with proper dependencies injected
        /// </summary>
        /// <param name="storageOptions">The configuration options</param>
        /// <param name="urlSlugGenerator">The URL Slug Generator</param>
        /// <param name="mimeTypeMapper">Mime-Type mapper for proper storage</param>
        /// <param name="logger">An ILogger for the current object</param>
        public AzureCloudStorageProvider(IOptions<AzureCloudStorageOptions> storageOptions,
            IUrlSlugGenerator urlSlugGenerator, IMimeTypeMapper mimeTypeMapper,
            ILogger<AzureCloudStorageProvider> logger)
        {
            _storageOptions = storageOptions;
            _urlSlugGenerator = urlSlugGenerator;
            _mimeTypeMapper = mimeTypeMapper;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<string> StoreObject(string desiredContainer, Stream fileContents, string desiredName)
        {
            //Get the client
            var blobClient = new BlobServiceClient(_storageOptions.Value.StorageConnectionString);

            //Create the client & container reference
            var container = blobClient.GetBlobContainerClient(desiredContainer.ToLower());

            //Get our block/blob reference
            var blockBlob = container.GetBlobClient(desiredName);
            _logger.LogInformation("Creating file {url} in Azure Blob Storage", blockBlob.Uri);

            //Delete existing if needed
            await blockBlob.DeleteIfExistsAsync().ConfigureAwait(false);

            //Determine content type
            _mimeTypeMapper.TryGetMimeType(desiredName, out var contentType);

            //Upload it
            if (string.IsNullOrEmpty(contentType))
                await blockBlob.UploadAsync(fileContents).ConfigureAwait(false);
            else
                await blockBlob.UploadAsync(fileContents, new BlobHttpHeaders {ContentType = contentType})
                    .ConfigureAwait(false);

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
            //Get the client
            var blobClient = new BlobServiceClient(_storageOptions.Value.StorageConnectionString);

            //Create the client & container reference
            var container = blobClient.GetBlobContainerClient(desiredContainer.ToLower());

            //Get our block/blob reference
            var blockBlob = container.GetBlobClient(desiredName);
            _logger.LogInformation("Creating file {url} in Azure Blob Storage", blockBlob.Uri);

            //Delete existing if needed
            await blockBlob.DeleteIfExistsAsync();

            //Determine content type
            _mimeTypeMapper.TryGetMimeType(desiredName, out var contentType);

            using (var memStream = new MemoryStream(fileContents, false))
            {
                if (string.IsNullOrEmpty(contentType))
                    await blockBlob.UploadAsync(memStream).ConfigureAwait(false);
                else
                    await blockBlob.UploadAsync(memStream, new BlobHttpHeaders {ContentType = contentType})
                        .ConfigureAwait(false);
            }

            return $"{_storageOptions.Value.RootClientPath}/{desiredContainer.ToLower()}/{desiredName}";
        }

        /// <inheritdoc />
        public async Task<bool> DeleteObject(string expectedContainer, string fullObjectUrl)
        {
            //Get object name
            var objectName = GetObjectName(expectedContainer, fullObjectUrl);
            if (objectName == null)
                return false;

            //Get the client
            var blobClient = new BlobServiceClient(_storageOptions.Value.StorageConnectionString);

            //Get the blob container & blob
            var container = blobClient.GetBlobContainerClient(expectedContainer.ToLower());
            var blockBlob = container.GetBlobClient(objectName);
            _logger.LogInformation($"Deleting {objectName} from {expectedContainer} storage container", objectName,
                expectedContainer);

            //Delete
            return await blockBlob.DeleteIfExistsAsync().ConfigureAwait(false);
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
        public BlobUriInfo DecomposeBlobUrl(string fullObjectPath)
        {
            //Validate that it was the root path
            if (!fullObjectPath.StartsWith(_storageOptions.Value.RootClientPath))
                throw new ArgumentException("Provided URI was not of the expected root path", nameof(fullObjectPath));

            var rootPath = _storageOptions.Value.RootClientPath;
            var containerAndObject = fullObjectPath.Replace(rootPath + "/", string.Empty);
            var container = containerAndObject.Substring(0, containerAndObject.IndexOf("/"));
            var objectName = containerAndObject.Replace(container + "/", string.Empty);
            return new BlobUriInfo
            {
                BlobName = objectName,
                Container = container,
                RootUrl = rootPath
            };
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
                cdnPath = await StoreObject(container, file.OpenReadStream(), targetFileName).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error uploading file with {slug} to Azure with error {message}", urlSlug, ex.Message);
                throw new FileLoadException($"Error Uploading to Azure: {ex.Message}");
            }

            return cdnPath;
        }

        /// <inheritdoc />
        public async Task<string> UploadIFormFile(IFormFile file, string container, string desiredName)
        {
            //Throw exception if no file
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            //Upload the file
            string cdnPath;
            try
            {
                cdnPath = await StoreObject(container, file.OpenReadStream(), desiredName).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error uploading file with desired name {desiredName} to Azure with error {message}",
                    desiredName, ex.Message);
                throw new FileLoadException($"Error Uploading to Azure: {ex.Message}");
            }

            return cdnPath;
        }

        /// <inheritdoc />
        public async Task<List<BlobInfo>> ListBlobs(string container)
        {
            //Get the client
            var blobClient = new BlobServiceClient(_storageOptions.Value.StorageConnectionString);

            //Get the blob container & blob
            var containerClient = blobClient.GetBlobContainerClient(container.ToLower());
            var blobList = new List<BlobInfo>();
            await foreach (var item in containerClient.GetBlobsAsync().ConfigureAwait(false))
                blobList.Add(new BlobInfo
                {
                    ContentType = item.Properties.ContentType,
                    CreatedDate = item.Properties.CreatedOn,
                    DownloadPath = $"{_storageOptions.Value.RootClientPath}/{container.ToLower()}/{item.Name}",
                    LastEditedDate = item.Properties.LastModified,
                    ObjectName = item.Name,
                    ObjectSize = item.Properties.ContentLength,
                    LastAccessDate = item.Properties.LastAccessedOn
                });

            return blobList;
        }

        /// <inheritdoc />
        public string CreateSASUrl(string fullObjectPath)
        {
            return CreateSASUrl(fullObjectPath, _storageOptions.Value.DefaultSASTokenDurationMinutes);
        }

        /// <inheritdoc />
        public string CreateSASUrl(string fullObjectPath, int tokenDuration)
        {
            var parts = DecomposeBlobUrl(fullObjectPath);
            return CreateSASUrl(parts.Container, parts.BlobName, tokenDuration);
        }

        /// <inheritdoc />
        public string CreateSASUrl(string container, string objectName)
        {
            return CreateSASUrl(container, objectName, _storageOptions.Value.DefaultSASTokenDurationMinutes);
        }

        /// <inheritdoc />
        public string CreateSASUrl(string container, string objectName, int tokenDuration)
        {
            var blobClient = new BlobClient(_storageOptions.Value.StorageConnectionString, container, objectName);
            if (blobClient.CanGenerateSasUri)
            {
                var tokenBuilder = new BlobSasBuilder
                {
                    BlobContainerName = container,
                    BlobName = objectName,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                };
                tokenBuilder.SetPermissions(BlobSasPermissions.Read);
                return blobClient.GenerateSasUri(tokenBuilder).ToString();
            }

            _logger.LogError("Azure Connection String not authorized with enough permissions to create a SAS");
            throw new InvalidOperationException("Azure Connection String doesn't support creating SAS URLs");
        }
    }
}