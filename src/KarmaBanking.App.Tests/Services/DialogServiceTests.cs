namespace KarmaBanking.App.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using KarmaBanking.App.Services;
    using Xunit;

    public class DialogServiceTests
    {
        [Fact]
        public async Task ShowConfirmDialogAsync_NullXamlRoot_ThrowsException()
        {
            var dialogService = new DialogService();

            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await dialogService.ShowConfirmDialogAsync("Title", "Message", "Yes", "No", null));
        }

        [Theory]
        [InlineData("", "", "", "")]
        [InlineData(null, null, null, null)]
        public async Task ShowConfirmDialogAsync_NullOrEmptyStrings_ThrowsException(string title, string message, string primary, string close)
        {
            var dialogService = new DialogService();

            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await dialogService.ShowConfirmDialogAsync(title, message, primary, close, null));
        }

        [Fact]
        public async Task ShowErrorDialogAsync_NullXamlRoot_ThrowsException()
        {
            var dialogService = new DialogService();

            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await dialogService.ShowErrorDialogAsync("Title", "Message", null));
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(null, null)]
        public async Task ShowErrorDialogAsync_NullOrEmptyStrings_ThrowsException(string title, string message)
        {
            var dialogService = new DialogService();

            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await dialogService.ShowErrorDialogAsync(title, message, null));
        }

        [Fact]
        public async Task ShowInputDialogAsync_NullXamlRoot_ThrowsException()
        {
            var dialogService = new DialogService();

            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await dialogService.ShowInputDialogAsync("Title", "Placeholder", "OK", "Cancel", null));
        }

        [Theory]
        [InlineData("", "", "", "")]
        [InlineData(null, null, null, null)]
        public async Task ShowInputDialogAsync_NullOrEmptyStrings_ThrowsException(string title, string placeholder, string primary, string close)
        {
            var dialogService = new DialogService();

            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await dialogService.ShowInputDialogAsync(title, placeholder, primary, close, null));
        }

        [Fact]
        public async Task ShowInfoDialogAsync_NullXamlRoot_ThrowsException()
        {
            var dialogService = new DialogService();

            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await dialogService.ShowInfoDialogAsync("Title", "Message", null));
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(null, null)]
        public async Task ShowInfoDialogAsync_NullOrEmptyStrings_ThrowsException(string title, string message)
        {
            var dialogService = new DialogService();

            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await dialogService.ShowInfoDialogAsync(title, message, null));
        }
    }
}