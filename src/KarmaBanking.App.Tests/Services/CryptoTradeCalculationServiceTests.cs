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
        public void TryParsePositiveQuantity_ValidPositiveQuantity_ReturnsTrueAndParsedValue()
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

        [Fact]
        public void TryParsePositiveQuantity_ZeroQuantity_ReturnsFalseAndZero()
        {
            // Arrange
            var cryptoCalculationService = new CryptoTradeCalculationService();

            // Act
            bool isParsedSuccessfully = cryptoCalculationService.TryParsePositiveQuantity("0", out decimal parsedQuantity);

            // Assert
            Assert.False(isParsedSuccessfully);
            Assert.Equal(0m, parsedQuantity);
        }

        [Fact]
        public void TryParsePositiveQuantity_NegativeQuantity_ReturnsFalseAndZero()
        {
            // Arrange
            var cryptoCalculationService = new CryptoTradeCalculationService();

            // Act
            bool isParsedSuccessfully = cryptoCalculationService.TryParsePositiveQuantity("-5", out decimal parsedQuantity);

            // Assert
            Assert.False(isParsedSuccessfully);
            Assert.Equal(0m, parsedQuantity);
        }

        [Fact]
        public void TryParsePositiveQuantity_CharactersInsteadOfQuantity_ReturnsFalseAndZero()
        {
            // Arrange
            var cryptoCalculationService = new CryptoTradeCalculationService();

            // Act
            bool isParsedSuccessfully = cryptoCalculationService.TryParsePositiveQuantity("abc", out decimal parsedQuantity);

            // Assert
            Assert.False(isParsedSuccessfully);
            Assert.Equal(0m, parsedQuantity);
        }

        [Fact]
        public void TryParsePositiveQuantity_EmptyStringInsteadOfDecimalQuantity_ReturnsFalseAndZero()
        {
            // Arrange
            var cryptoCalculationService = new CryptoTradeCalculationService();

            // Act
            bool isParsedSuccessfully = cryptoCalculationService.TryParsePositiveQuantity(string.Empty, out decimal parsedQuantity);

            // Assert
            Assert.False(isParsedSuccessfully);
            Assert.Equal(0m, parsedQuantity);
        }

        [Fact]
        public void TryParsePositiveQuantity_NullQuantity_ReturnsFalseAndZero()
        {
            // Arrange
            var cryptoCalculationService = new CryptoTradeCalculationService();

            // Act
            bool isParsedSuccessfully = cryptoCalculationService.TryParsePositiveQuantity(null, out decimal parsedQuantity);

            // Assert
            Assert.False(isParsedSuccessfully);
            Assert.Equal(0m, parsedQuantity);
        }

        [Fact]
        public void GetMockMarketPrice_BitcoinTicker_ReturnsExpectedPrice()
        {
            // Arrange
            var cryptoCalculationService = new CryptoTradeCalculationService();

            // Act
            decimal marketPrice = cryptoCalculationService.GetMockMarketPrice("BTC");

            // Assert
            Assert.Equal(65000m, marketPrice);
        }

        [Fact]
        public void CalculateTradePreview_BuyActionAboveMinimumFee_CalculatesCorrectly()
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
        public void CanExecuteTrade_BuyWithSufficientFunds_ReturnsTrue()
        {
            // Arrange
            var cryptoCalculationService = new CryptoTradeCalculationService();

            // Act
            bool result = cryptoCalculationService.CanExecuteTrade(false, "1", "BUY", 4000m, 5000m);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanExecuteTrade_OtherActionType_ReturnsTrue()
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