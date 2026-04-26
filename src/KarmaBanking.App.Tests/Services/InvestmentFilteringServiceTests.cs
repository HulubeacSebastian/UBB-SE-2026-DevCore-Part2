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
        public void FilterHoldingsByAssetType_NullHoldings_ReturnsEmpty()
        {
            // Arrange
            var investmentFilteringService = new InvestmentFilteringService();

            // Act
            var filteredHoldingsResult = investmentFilteringService.FilterHoldingsByAssetType(null, "Stocks");

            // Assert
            Assert.Empty(filteredHoldingsResult);
        }

        [Theory]
        [InlineData("Stock", "Stocks", true)]
        [InlineData("ETF", "Stocks", false)]
        [InlineData("Crypto", "Crypto", true)]
        public void FilterHoldingsByAssetType_VariousFilters_ReturnsExpectedMatch(
            string assetTypeName,
            string filterCategoryName,
            bool shouldMatchAsset)
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
            if (shouldMatchAsset)
            {
                Assert.Single(filteredHoldingsResult);
            }
            else
            {
                Assert.Empty(filteredHoldingsResult);
            }
        }
    }
}