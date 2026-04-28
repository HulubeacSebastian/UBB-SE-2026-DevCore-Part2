// <copyright file="InvestmentFilteringServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using System.Collections.Generic;
    using global::KarmaBanking.App.Models;
    using global::KarmaBanking.App.Services;
    using Xunit;

    public class InvestmentFilteringServiceTests
    {
        [Fact]
        public void FilterHoldingsByAssetType_WhenHoldingsAreNull_ThenReturnsEmpty()
        {
            // Arrange
            var investmentFilteringService = new InvestmentFilteringService();

            // Act
            var filteredHoldingsResult = investmentFilteringService.FilterHoldingsByAssetType(null, "Stocks");

            // Assert
            Assert.Empty(filteredHoldingsResult);
        }

        [Theory]
        [InlineData("Stock", "Stocks")]
        [InlineData("Crypto", "Crypto")]
        public void FilterHoldingsByAssetType_MatchingAsset_ReturnsSingleHolding(
            string assetTypeName,
            string filterCategoryName)
        {
            // Arrange
            var investmentFilteringService = new InvestmentFilteringService();
            var holdingEntries = new List<InvestmentHolding>
            {
                new InvestmentHolding { AssetType = assetTypeName }
            };

            // Act
            var filteredHoldingsResult = investmentFilteringService.FilterHoldingsByAssetType(holdingEntries, filterCategoryName);

            // Assert
            Assert.Single(filteredHoldingsResult);
        }

        [Theory]
        [InlineData("ETF", "Stocks")]
        [InlineData("Crypto", "Stocks")]
        public void FilterHoldingsByAssetType_NonMatchingAsset_ReturnsEmptyList(
            string assetTypeName,
            string filterCategoryName)
        {
            // Arrange
            var investmentFilteringService = new InvestmentFilteringService();
            var holdingEntries = new List<InvestmentHolding>
            {
                new InvestmentHolding { AssetType = assetTypeName }
            };

            // Act
            var filteredHoldingsResult = investmentFilteringService.FilterHoldingsByAssetType(holdingEntries, filterCategoryName);

            // Assert
            Assert.Empty(filteredHoldingsResult);
        }
    }
}