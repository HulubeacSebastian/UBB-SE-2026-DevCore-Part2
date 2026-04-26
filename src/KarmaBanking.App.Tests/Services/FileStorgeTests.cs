// <copyright file="FileStorageTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using global::KarmaBanking.App.Services;
    using Xunit;

    public class FileStorageTests : IDisposable
    {
        private readonly FileStorage fileStorage;
        private readonly string testFilesDirectoryPath;

        public FileStorageTests()
        {
            this.fileStorage = new FileStorage();
            this.testFilesDirectoryPath = Path.Combine(Path.GetTempPath(), "KarmaBankingTestFiles_" + Guid.NewGuid());
            Directory.CreateDirectory(this.testFilesDirectoryPath);
        }

        [Fact]
        public async Task UploadFileAsync_ThrowsArgumentException_WhenPathIsNullOrWhitespace()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => this.fileStorage.UploadFileAsync(string.Empty));
        }

        [Fact]
        public async Task UploadFileAsync_SuccessfullyCopiesFile_WhenFileIsValid()
        {
            // Arrange
            string validFilePath = Path.Combine(this.testFilesDirectoryPath, "validimage.png");
            await File.WriteAllTextAsync(validFilePath, "fake image content");

            // Act
            string destinationPathResult = await this.fileStorage.UploadFileAsync(validFilePath);

            // Assert
            Assert.NotNull(destinationPathResult);
            Assert.True(File.Exists(destinationPathResult));
            Assert.EndsWith(".png", destinationPathResult);
        }

        [Fact]
        public async Task DeleteUrl_RemovesFile_WhenFileExists()
        {
            // Arrange
            string fileToDeletePath = Path.Combine(this.testFilesDirectoryPath, "todelete.pdf");
            await File.WriteAllTextAsync(fileToDeletePath, "dummy content");

            // Act
            this.fileStorage.DeleteUrl(fileToDeletePath);

            // Assert
            Assert.False(File.Exists(fileToDeletePath));
        }

        public void Dispose()
        {
            if (Directory.Exists(this.testFilesDirectoryPath))
            {
                Directory.Delete(this.testFilesDirectoryPath, true);
            }
        }
    }
}