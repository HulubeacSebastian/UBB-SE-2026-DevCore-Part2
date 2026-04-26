// <copyright file="MarketDataServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

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
            // Arrange
            var marketDataService = new MarketDataService();

            // Act
            decimal currentMarketPrice = marketDataService.GetPrice("BTC");

            // Assert
            Assert.Equal(68000m, currentMarketPrice);
        }

        [Fact]
        public void StartPolling_FiltersAndNormalizesTickers()
        {
            // Arrange
            var marketDataService = new MarketDataService();
            var messyTickerSymbolsList = new List<string> { " btc ", string.Empty, null!, "AAPL", "btc" };

            // Act
            marketDataService.StartPolling(messyTickerSymbolsList);

            // Assert
            Assert.Equal(68000m, marketDataService.GetPrice("BTC"));
            Assert.Equal(185m, marketDataService.GetPrice("aapl"));

            marketDataService.StopPolling();
        }
    }
}