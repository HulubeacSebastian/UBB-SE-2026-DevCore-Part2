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
        public async Task ShowConfirmDialogAsync_WhenXamlRootIsNull_ThenThrowsException()
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
        public async Task ShowConfirmDialogAsync_WhenParametersAreNullOrEmpty_ThenThrowsException(
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
        public async Task ShowErrorDialogAsync_WhenXamlRootIsNull_ThenThrowsException()
        {
            // Arrange
            var dialogService = new DialogService();

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await dialogService.ShowErrorDialogAsync("Error", "Details", null));
        }
    }
}