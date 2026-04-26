// <copyright file="DialogServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using global::KarmaBanking.App.Services;
    using Xunit;

    public class DialogServiceTests
    {
        [Fact]
        public async Task ShowConfirmDialogAsync_NullXamlRoot_ThrowsException()
        {
            // Arrange
            var dialogService = new DialogService();

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await dialogService.ShowConfirmDialogAsync("Title", "Message", "Yes", "No", null));
        }

        [Theory]
        [InlineData("", "", "", "")]
        [InlineData(null, null, null, null)]
        public async Task ShowConfirmDialogAsync_NullOrEmptyStrings_ThrowsException(
            string titleValue,
            string messageValue,
            string primaryButtonText,
            string closeButtonText)
        {
            // Arrange
            var dialogService = new DialogService();

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await dialogService.ShowConfirmDialogAsync(titleValue, messageValue, primaryButtonText, closeButtonText, null));
        }

        [Fact]
        public async Task ShowErrorDialogAsync_NullXamlRoot_ThrowsException()
        {
            // Arrange
            var dialogService = new DialogService();

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await dialogService.ShowErrorDialogAsync("Error", "Details", null));
        }
    }
}