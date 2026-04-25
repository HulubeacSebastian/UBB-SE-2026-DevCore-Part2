namespace KarmaBanking.App.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using KarmaBanking.App.Services;
    using Xunit;

    public class FileValidationServiceTests
    {
        [Fact]
        public void GetFileSizeDisplay_BytesSize_ReturnsBytesFormatted()
        {
            long fileSize = 500;
            var fileSizeString = FileValidationService.GetFileSizeDisplay(fileSize);
            Assert.Equal("500 B", fileSizeString);
        }

        [Fact]
        public void GetFileSizeDisplay_ExactKilobyte_ReturnsKilobytesFormatted()
        {
            long fileSize = 1024;
            var fileSizeString = FileValidationService.GetFileSizeDisplay(fileSize);
            Assert.Equal("1 KB", fileSizeString);
        }

        [Fact]
        public void GetFileSizeDisplay_KilobytesSize_ReturnsKilobytesFormatted()
        {
            long fileSize = 1536;
            var fileSizeString = FileValidationService.GetFileSizeDisplay(fileSize);
            Assert.Equal("1.5 KB", fileSizeString);
        }

        [Fact]
        public void GetFileSizeDisplay_ExactMegabyte_ReturnsMegabytesFormatted()
        {
            long fileSize = 1048576;
            var fileSizeString = FileValidationService.GetFileSizeDisplay(fileSize);
            Assert.Equal("1 MB", fileSizeString);
        }

        [Fact]
        public void GetFileSizeDisplay_MegabytesSize_ReturnsMegabytesFormatted()
        {
            long fileSize = 2621440;
            var fileSizeString = FileValidationService.GetFileSizeDisplay(fileSize);
            Assert.Equal("2.5 MB", fileSizeString);
        }

        [Fact]
        public async Task ValidateFileAsync_NullFile_ReturnsFalseAndErrorMessage()
        {
            var fileValidator = new FileValidationService();
            var (isValid, errorMessage) = await fileValidator.ValidateFileAsync(null);

            Assert.False(isValid);
            Assert.Equal("No file selected.", errorMessage);
        }

        [Fact]
        public async Task MapStorageFileToAttachmentAsync_NullFile_ThrowsInvalidOperationException()
        {
            var fileValidator = new FileValidationService();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await fileValidator.MapStorageFileToAttachmentAsync(null));

            Assert.Contains("Failed to map file to attachment", exception.Message);
        }
    }
}