// <copyright file="CryptoTradeCalculationServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using global::KarmaBanking.App.Services;
    using Xunit;

    public class CryptoTradeCalculationServiceTests
    {
        [Fact]
        public void TryParsePositiveQuantity_WhenValidPositiveQuantity_ThenReturnsTrueAndParsedValue()
        {
            // Arrange
            var cryptoCalculationService = new CryptoTradeCalculationService();
            string quantityTextValue = "10.5";

            // Act
            bool isParsedSuccessfully = cryptoCalculationService.TryParsePositiveQuantity(quantityTextValue, out decimal parsedQuantity);

            // Assert
            Assert.True(isParsedSuccessfully);
            Assert.Equal(10.5m, parsedQuantity);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-5")]
        [InlineData("abc")]
        [InlineData("")]
        [InlineData(null)]
        public void TryParsePositiveQuantity_WhenInvalidOrNonPositiveQuantity_ThenReturnsFalseAndZero(string quantityTextValue)
        {
            // Arrange
            var cryptoCalculationService = new CryptoTradeCalculationService();

            // Act
            bool isParsedSuccessfully = cryptoCalculationService.TryParsePositiveQuantity(quantityTextValue, out decimal parsedQuantity);

            // Assert
            Assert.False(isParsedSuccessfully);
            Assert.Equal(0m, parsedQuantity);
        }

        [Fact]
        public void GetMockMarketPrice_WhenGivenBitcoinTicker_ThenReturnsExpectedPrice()
        {
            // Arrange
            var cryptoCalculationService = new CryptoTradeCalculationService();

            // Act
            decimal marketPrice = cryptoCalculationService.GetMockMarketPrice("BTC");

            // Assert
            Assert.Equal(65000m, marketPrice);
        }

        [Fact]
        public void CalculateTradePreview_WhenBuyActionAboveMinimumFee_ThenCalculatesCorrectly()
        {
            // Arrange
            var cryptoCalculationService = new CryptoTradeCalculationService();

            // Act
            var (estimatedFee, totalAmount) = cryptoCalculationService.CalculateTradePreview("BTC", "BUY", 1m);

            // Assert
            Assert.Equal(975m, estimatedFee);
            Assert.Equal(65975m, totalAmount);
        }

        [Fact]
        public void CanExecuteTrade_WhenBuyWithSufficientFunds_ThenReturnsTrue()
        {
            // Arrange
            var cryptoCalculationService = new CryptoTradeCalculationService();

            // Act
            bool result = cryptoCalculationService.CanExecuteTrade(false, "1", "BUY", 4000m, 5000m);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanExecuteTrade_WhenActionTypeIsConvert_ThenReturnsTrue()
        {
            // Arrange
            var cryptoCalculationService = new CryptoTradeCalculationService();

            // Act
            bool result = cryptoCalculationService.CanExecuteTrade(false, "1", "CONVERT", 10000m, 5000m);

            // Assert
            Assert.True(result);
        }
    }
}