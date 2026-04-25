namespace KarmaBanking.App.Tests.Services
{
    using KarmaBanking.App.Services;
    using Xunit;

    public class CryptoTradeCalculationServiceTests
    {
        [Fact]
        public void TryParsePositiveQuantity_ValidPositiveQuantity_ReturnsTrueAndParsedValue()
        {
            var cryptoCalculatorService = new CryptoTradeCalculationService();
            var quantityText = "10.5";
            var quantityDecimal = 10.5m;

            var succesfulyParsedQuantityString = cryptoCalculatorService.TryParsePositiveQuantity(quantityText, out var quantity);

            Assert.True(succesfulyParsedQuantityString);
            Assert.Equal(quantityDecimal, quantity);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-5")]
        [InlineData("abc")]
        [InlineData("")]
        [InlineData(null)]
        public void TryParsePositiveQuantity_InvalidOrNonPositiveQuantity_ReturnsFalseAndZero(string quantityText)
        {
            var cryptoCaluclatorService = new CryptoTradeCalculationService();

            var succesfulltParsedQuantityString = cryptoCaluclatorService.TryParsePositiveQuantity(quantityText, out var quantity);
            var invalidQuantity = 0m;

            Assert.False(succesfulltParsedQuantityString);
            Assert.Equal(invalidQuantity, quantity);
        }

        [Fact]
        public void GetMockMarketPrice_BtcTicker_Returns65000()
        {
            var cryptoCaluclatorService = new CryptoTradeCalculationService();
            var expectedPrice = cryptoCaluclatorService.GetMockMarketPrice("BTC");
            var priceBTC = 65000m;
            Assert.Equal(priceBTC, expectedPrice);
        }

        [Fact]
        public void GetMockMarketPrice_OtherTicker_Returns3000()
        {
            var cryptoCaluclatorService = new CryptoTradeCalculationService();
            var expectedPrice = cryptoCaluclatorService.GetMockMarketPrice("ETH");
            var priceETH = 65000m;
            Assert.Equal(priceETH, expectedPrice);
        }

        [Fact]
        public void CalculateTradePreview_BuyActionAboveMinimumFee_CalculatesCorrectly()
        {
            var cryptoCaluclatorService = new CryptoTradeCalculationService();
            var amountBoughtBTC = 1m;
            var (estimatedFee, totalAmount) = cryptoCaluclatorService.CalculateTradePreview("BTC", "BUY", amountBoughtBTC);
            var feeForBuyingBTC = 975m;
            var totalPriceOfBTC = 65975m;
            Assert.Equal(feeForBuyingBTC, estimatedFee);
            Assert.Equal(totalPriceOfBTC, totalAmount);
        }

        [Fact]
        public void CalculateTradePreview_SellActionBelowMinimumFee_AppliesMinimumFee()
        {
            var cryptoCaluclatorService = new CryptoTradeCalculationService();
            var amountBoughtETH = 0.001m;
            var (estimatedFee, totalAmount) = cryptoCaluclatorService.CalculateTradePreview("ETH", "SELL", amountBoughtETH);
            var feeForBuyingETH = 0.50m;
            var totalPriceOfEth = 2.50m;
            Assert.Equal(feeForBuyingETH, estimatedFee);
            Assert.Equal(totalPriceOfEth, totalAmount);
        }

        [Fact]
        public void CanExecuteTrade_IsSubmittingTrue_ReturnsFalse()
        {
            var cryptoCaluclatorServiceservice = new CryptoTradeCalculationService();
            var priceToBePaid = 1000m;
            var userBalance = 5000m;
            var canExecuteTrade = cryptoCaluclatorServiceservice.CanExecuteTrade(true, "1", "BUY", priceToBePaid, userBalance);
            Assert.False(canExecuteTrade);
        }

        [Fact]
        public void CanExecuteTrade_InvalidQuantity_ReturnsFalse()
        {
            var cryptoCaluclatorService = new CryptoTradeCalculationService();
            var priceToBePaid = 1000m;
            var userBalance = 5000m;
            var canExecuteTrade = cryptoCaluclatorService.CanExecuteTrade(false, "abc", "BUY", priceToBePaid, userBalance);
            Assert.False(canExecuteTrade);
        }

        [Fact]
        public void CanExecuteTrade_BuyWithInsufficientFunds_ReturnsFalse()
        {
            var cryptoCaluclatorService = new CryptoTradeCalculationService();
            var priceToBePaid = 6000m;
            var userBalance = 5000m;
            var canExecuteTrade = cryptoCaluclatorService.CanExecuteTrade(false, "1", "BUY", priceToBePaid, userBalance);
            Assert.False(canExecuteTrade);
        }

        [Fact]
        public void CanExecuteTrade_BuyWithSufficientFunds_ReturnsTrue()
        {
            var cryptoCaluclatorService = new CryptoTradeCalculationService();
            var priceToBePaid = 4000m;
            var userBalance = 5000m;
            var canExecuteTrade = cryptoCaluclatorService.CanExecuteTrade(false, "1", "BUY", priceToBePaid, userBalance);
            Assert.True(canExecuteTrade);
        }

        [Fact]
        public void CanExecuteTrade_SellAction_ReturnsTrueRegardlessOfBalance()
        {
            var cryptoCaluclatorService = new CryptoTradeCalculationService();
            var sellingAmount = 10000m;
            var userBalance = 5000m;
            var canExecuteTrade = cryptoCaluclatorService.CanExecuteTrade(false, "1", "SELL", sellingAmount, userBalance);
            Assert.True(canExecuteTrade);
        }
    }
}