// <copyright file="FileValidationServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using global::KarmaBanking.App.Services;
    using Xunit;

    public class FileValidationServiceTests
    {
        [Fact]
        public void GetFileSizeDisplay_ExactKilobyte_ReturnsKilobytesFormatted()
        {
            // Arrange
            long sizeInBytes = 1024;

            // Act
            string formattedDisplaySize = FileValidationService.GetFileSizeDisplay(sizeInBytes);

            // Assert
            Assert.Equal("1 KB", formattedDisplaySize);
        }

        [Fact]
        public async Task ValidateFileAsync_NullFile_ReturnsFalseAndErrorMessage()
        {
            // Arrange
            var fileValidationService = new FileValidationService();

            // Act
            var (isValidResult, validationErrorMessage) = await fileValidationService.ValidateFileAsync(null);

            // Assert
            Assert.False(isValidResult);
            Assert.Equal("No file selected.", validationErrorMessage);
        }

        [Fact]
        public async Task MapStorageFileToAttachmentAsync_NullFile_ThrowsInvalidOperationException()
        {
            // Arrange
            var fileValidationService = new FileValidationService();

            // Act & Assert
            var validationException = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await fileValidationService.MapStorageFileToAttachmentAsync(null));

            Assert.Contains("Failed to map file to attachment", validationException.Message);
        }
    }
}