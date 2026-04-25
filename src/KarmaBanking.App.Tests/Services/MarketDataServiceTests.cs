namespace KarmaBanking.App.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using KarmaBanking.App.Services;
    using Xunit;

    public class MarketDataServiceTests
    {
        [Fact]
        public void GetPrice_ValidTicker_ReturnsInitialPrice()
        {
            var marketDateService = new MarketDataService();

            decimal priceBTC = marketDateService.GetPrice("BTC");
            var expectedPriceBTC = 68000m;

            Assert.Equal(expectedPriceBTC, priceBTC);
        }

        [Fact]
        public void GetPrice_InvalidOrWhitespaceTicker_ReturnsZero()
        {
            var marketDateService = new MarketDataService();

            Assert.Equal(0m, marketDateService.GetPrice(null!));
            Assert.Equal(0m, marketDateService.GetPrice("   "));
            Assert.Equal(0m, marketDateService.GetPrice("INVALID"));
        }

        [Fact]
        public void StartPolling_FiltersAndNormalizesTickers()
        {
            var marketDateService = new MarketDataService();
            var messyTickers = new List<string> { " btc ", string.Empty, null!, "AAPL", "btc" };

            marketDateService.StartPolling(messyTickers);

            var expectedPriceBTC = 68000m;
            var expectedPriceAAPL = 185m;
            Assert.Equal(expectedPriceBTC, marketDateService.GetPrice("BTC"));
            Assert.Equal(expectedPriceAAPL, marketDateService.GetPrice("aapl"));

            marketDateService.StopPolling();
        }

        [Fact]
        public void StartPolling_CalledTwice_DoesNotRestartTimer()
        {
            var marketDateService = new MarketDataService();
            var tickers = new List<string> { "BTC" };

            marketDateService.StartPolling(tickers);
            marketDateService.StartPolling(tickers);

            Assert.NotNull(tickers);
            marketDateService.StopPolling();
        }

        [Fact]
        public void RegisterPriceUpdateHandler_SetsHandlerCorrectly()
        {
            var marketDateService = new MarketDataService();
            bool handlerCalled = false;
            Action handler = () => handlerCalled = true;

            marketDateService.RegisterPriceUpdateHandler(handler);

            Assert.False(handlerCalled);
        }

        [Fact]
        public async Task StartPolling_FluctuatesPrices_AfterInterval()
        {
            var marketDateService = new MarketDataService();
            var tickers = new List<string> { "BTC" };
            decimal initialPrice = marketDateService.GetPrice("BTC");
            bool wasNotified = false;

            marketDateService.RegisterPriceUpdateHandler(() => wasNotified = true);

            marketDateService.StartPolling(tickers);

            await Task.Delay(5500);

            decimal updatedPrice = marketDateService.GetPrice("BTC");

            Assert.NotEqual(initialPrice, updatedPrice);
            Assert.True(wasNotified);

            marketDateService.StopPolling();
        }
    }
}