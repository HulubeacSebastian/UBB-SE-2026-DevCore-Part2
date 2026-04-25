namespace KarmaBanking.App.Tests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using KarmaBanking.App.Models;
    using KarmaBanking.App.Services;
    using Xunit;

    public class InvestmentFilteringServiceTests
    {
        [Fact]
        public void FilterHoldingsByAssetType_NullHoldings_ReturnsEmpty()
        {
            var investmentFilter = new InvestmentFilteringService();
            var holdingsEnumerable = investmentFilter.FilterHoldingsByAssetType(null, "Stocks");
            Assert.Empty(holdingsEnumerable);
        }

        [Fact]
        public void FilterHoldingsByAssetType_NullHoldingElement_IgnoresNull()
        {
            var investmentFilter = new InvestmentFilteringService();
            var holdingListEmpty = new List<InvestmentHolding> { null };

            var holdingsEnumerable = investmentFilter.FilterHoldingsByAssetType(holdingListEmpty, "Stocks");

            Assert.Empty(holdingsEnumerable);
        }

        [Theory]
        [InlineData("Stock", "Stocks", true)]
        [InlineData("Stocks", "Stocks", true)]
        [InlineData("ETF", "Stocks", false)]
        [InlineData("ETF", "ETFs", true)]
        [InlineData("ETFs", "ETFs", true)]
        [InlineData("Bond", "Bonds", true)]
        [InlineData("Bonds", "Bonds", true)]
        [InlineData("Crypto", "Crypto", true)]
        [InlineData("Real Estate", "Other", true)]
        [InlineData("Stock", "Other", false)]
        [InlineData("Commodities", "All", true)]
        public void FilterHoldingsByAssetType_VariousFilters_ReturnsExpectedMatch(string assetType, string filter, bool shouldMatch)
        {
            var investmentFilter = new InvestmentFilteringService();
            var holdingListPopulated = new List<InvestmentHolding>
            {
                new InvestmentHolding { AssetType = assetType }
            };

            var holdingsEnumerable = investmentFilter.FilterHoldingsByAssetType(holdingListPopulated, filter);

            if (shouldMatch)
            {
                Assert.Single(holdingsEnumerable);
            }
            else
            {
                Assert.Empty(holdingsEnumerable);
            }
        }
    }
}