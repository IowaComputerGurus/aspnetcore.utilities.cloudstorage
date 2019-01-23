using Xunit;

namespace ICG.AspNetCore.Utilities.CloudStorage.Tests
{
    public class MimeTypeMapperTests
    {
        private readonly IMimeTypeMapper _mimeTypeMapper;

        public MimeTypeMapperTests()
        {
            _mimeTypeMapper = new MimeTypeMapper();
        }

        [Theory]
        [InlineData("test.png", true, "image/png")]
        [InlineData("test.jpg", true, "image/jpeg")]
        [InlineData("test.zzz", false, null)]
        public void TryGetMimeType_ShouldReturnProperResultAndType(string input, bool expectedResult,
            string expectedValue)
        {
            //Act
            var actualResult = _mimeTypeMapper.TryGetMimeType(input, out var mimeType);

            //Assert
            Assert.Equal(expectedResult, actualResult);
            Assert.Equal(expectedValue, mimeType);
        }
    }
}