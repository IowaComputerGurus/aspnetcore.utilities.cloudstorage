using System;
using ICG.NetCore.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ICG.AspNetCore.Utilities.CloudStorage.Tests
{
    public class AzureCloudStorageProviderTests
    {
        private readonly IAzureCloudStorageProvider _azureCloudStorageProvider;
        private readonly Mock<IUrlSlugGenerator> _urlSlugGenerator;
        private readonly Mock<IOptions<AzureCloudStorageOptions>> _azureCloudStorageOptionsMock;
        private readonly Mock<IMimeTypeMapper> _mimeTypeMapperMock;
        private readonly Mock<ILogger<AzureCloudStorageProvider>> _loggerMock;

        public AzureCloudStorageProviderTests()
        {
            var options = new AzureCloudStorageOptions {RootClientPath = "https://teststorage.blob.core.windows.net"};
            _azureCloudStorageOptionsMock = new Mock<IOptions<AzureCloudStorageOptions>>();
            _azureCloudStorageOptionsMock.Setup(s => s.Value)
                .Returns(options);
            _urlSlugGenerator = new Mock<IUrlSlugGenerator>();
            _mimeTypeMapperMock = new Mock<IMimeTypeMapper>();
            _loggerMock = new Mock<ILogger<AzureCloudStorageProvider>>();
            _azureCloudStorageProvider = new AzureCloudStorageProvider(_azureCloudStorageOptionsMock.Object,
                _urlSlugGenerator.Object, _mimeTypeMapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void GetObjectNameShouldReturnNullIfContainerNotMatching()
        {
            //Arrange
            var path = "https://teststorage.blob.core.windows.net/blog/testHeader.jpg";
            var container = "nothere";

            //Act
            var result = _azureCloudStorageProvider.GetObjectName(container, path);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetObjectNameShouldReturnFilePathForSimplePaths()
        {
            //Arrange
            var path = "https://teststorage.blob.core.windows.net/blog/testHeader.jpg";
            var container = "blog";
            var expectedResult = "testHeader.jpg";

            //Act
            var result = _azureCloudStorageProvider.GetObjectName(container, path);

            //Asset
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void GetObjectNameShouldReturnFilePathForComplexPaths()
        {
            //Arrange
            var path =
                "https://teststorage.blob.core.windows.net/blog/specialpath/testHeader.jpg";
            var container ="blog";
            var expectedResult = "specialpath/testHeader.jpg";

            //Act
            var result = _azureCloudStorageProvider.GetObjectName(container, path);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void DecomposeBlobUrlShouldReturnProperValues()
        {
            //Arrange
            var path = "https://teststorage.blob.core.windows.net/blog/specialpath/testHeader.jpg";
            var expectedDomain = "https://teststorage.blob.core.windows.net";
            var expectedContainer = "blog";
            var expectedBlob = "specialpath/testHeader.jpg";

            //Act
            var result = _azureCloudStorageProvider.DecomposeBlobUrl(path);

            //Assert
            Assert.Equal(expectedBlob, result.BlobName);
            Assert.Equal(expectedContainer, result.Container);
            Assert.Equal(expectedDomain, result.RootUrl);
        }

        [Fact]
        public void DecomposeBlobUrlShouldThrowExceptionForDifferentRootUrl()
        {
            //Arrange
            var path = "https://teststorage2.blob.core.windows.net/blog/specialpath/testHeader.jpg";

            //Act
            Assert.Throws<ArgumentException>(() => _azureCloudStorageProvider.DecomposeBlobUrl(path));
        }
    }
}