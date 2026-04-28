// <copyright file="PortfolioValuationServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using System.Collections.Generic;
    using KarmaBanking.App.Models;
    using KarmaBanking.App.Services;
    using Xunit;

    public class PortfolioValuationServiceTests
    {
        private const decimal AveragePurchasePrice = 50m;
        private const decimal PrimaryHoldingQuantity = 10m;
        private const decimal UpdatedMarketPrice = 75m;
        private const decimal ExpectedUnrealizedGainLoss = 250m;

        private const decimal SecondaryHoldingQuantity = 5m;
        private const decimal FirstHoldingAveragePurchasePrice = 100m;
        private const decimal FirstHoldingCurrentPrice = 150m;
        private const decimal FirstHoldingUnrealizedGainLoss = 500m;
        private const decimal SecondHoldingCurrentPrice = 40m;
        private const decimal SecondHoldingUnrealizedGainLoss = -50m;

        private const decimal ExpectedTotalValue = 1700m;
        private const decimal ExpectedTotalGainLoss = 450m;
        private const decimal ExpectedGainLossPercent = 36m;

        private readonly PortfolioValuationService portfolioValuationService;

        public PortfolioValuationServiceTests()
        {
            this.portfolioValuationService = new PortfolioValuationService();
        }

        [Fact]
        public void UpdateHoldingValuation_WhenNewMarketPriceProvided_ThenUpdatesPriceAndCalculatesGainLoss()
        {
            // Arrange
            var investmentHoldingInstance = new InvestmentHolding
            {
                AveragePurchasePrice = AveragePurchasePrice,
                Quantity = PrimaryHoldingQuantity
            };
            decimal newMarketPriceValue = UpdatedMarketPrice;

            // Act
            this.portfolioValuationService.UpdateHoldingValuation(investmentHoldingInstance, newMarketPriceValue);

            // Assert
            Assert.Equal(UpdatedMarketPrice, investmentHoldingInstance.CurrentPrice);
            Assert.Equal(ExpectedUnrealizedGainLoss, investmentHoldingInstance.UnrealizedGainLoss);
        }

        [Fact]
        public void UpdatePortfolioTotals_WhenMultipleHoldingsWithPositiveTotalCost_ThenCalculatesTotalsCorrectly()
        {
            // Arrange
            var userPortfolioInstance = new Portfolio
            {
                Holdings = new List<InvestmentHolding>
                {
                    new InvestmentHolding
                    {
                        Quantity = PrimaryHoldingQuantity,
                        AveragePurchasePrice = FirstHoldingAveragePurchasePrice,
                        CurrentPrice = FirstHoldingCurrentPrice,
                        UnrealizedGainLoss = FirstHoldingUnrealizedGainLoss
                    },
                    new InvestmentHolding
                    {
                        Quantity = SecondaryHoldingQuantity,
                        AveragePurchasePrice = AveragePurchasePrice,
                        CurrentPrice = SecondHoldingCurrentPrice,
                        UnrealizedGainLoss = SecondHoldingUnrealizedGainLoss
                    }
                }
            };

            // Act
            this.portfolioValuationService.UpdatePortfolioTotals(userPortfolioInstance);

            // Assert
            Assert.Equal(ExpectedTotalValue, userPortfolioInstance.TotalValue);
            Assert.Equal(ExpectedTotalGainLoss, userPortfolioInstance.TotalGainLoss);
            Assert.Equal(ExpectedGainLossPercent, userPortfolioInstance.GainLossPercent);
        }
    }
}