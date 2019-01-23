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

        public AzureCloudStorageProviderTests()
        {
            _azureCloudStorageOptionsMock = new Mock<IOptions<AzureCloudStorageOptions>>();
            _urlSlugGenerator = new Mock<IUrlSlugGenerator>();
            _mimeTypeMapperMock = new Mock<IMimeTypeMapper>();
            _azureCloudStorageProvider = new AzureCloudStorageProvider(_azureCloudStorageOptionsMock.Object,
                _urlSlugGenerator.Object,
                _mimeTypeMapperMock.Object);
        }

        [Fact]
        public void GetObjectNameShouldReturnNullIfContainerNotMatching()
        {
            //Arrange
            var path = "https:/teststorage.blob.core.windows.net/blog/testHeader.jpg";
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
            var path = "https:/teststorage.blob.core.windows.net/blog/testHeader.jpg";
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
                "https:/teststorage.blob.core.windows.net/blog/specialpath/testHeader.jpg";
            var container ="blog";
            var expectedResult = "specialpath/testHeader.jpg";

            //Act
            var result = _azureCloudStorageProvider.GetObjectName(container, path);

            //Assert
            Assert.Equal(expectedResult, result);
        }
    }
}