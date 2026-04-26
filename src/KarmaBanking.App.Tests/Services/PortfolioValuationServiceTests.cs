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
        private readonly PortfolioValuationService portfolioValuationService;

        public PortfolioValuationServiceTests()
        {
            this.portfolioValuationService = new PortfolioValuationService();
        }

        [Fact]
        public void UpdateHoldingValuation_UpdatesPriceAndCalculatesGainLoss()
        {
            // Arrange
            var investmentHoldingInstance = new InvestmentHolding
            {
                AveragePurchasePrice = 50m,
                Quantity = 10m
            };
            decimal newMarketPriceValue = 75m;

            // Act
            this.portfolioValuationService.UpdateHoldingValuation(investmentHoldingInstance, newMarketPriceValue);

            // Assert
            Assert.Equal(75m, investmentHoldingInstance.CurrentPrice);
            Assert.Equal(250m, investmentHoldingInstance.UnrealizedGainLoss);
        }

        [Fact]
        public void UpdatePortfolioTotals_WithPositiveTotalCost_CalculatesTotalsCorrectly()
        {
            // Arrange
            var userPortfolioInstance = new Portfolio
            {
                Holdings = new List<InvestmentHolding>
                {
                    new InvestmentHolding
                    {
                        Quantity = 10m,
                        AveragePurchasePrice = 100m,
                        CurrentPrice = 150m,
                        UnrealizedGainLoss = 500m
                    },
                    new InvestmentHolding
                    {
                        Quantity = 5m,
                        AveragePurchasePrice = 50m,
                        CurrentPrice = 40m,
                        UnrealizedGainLoss = -50m
                    }
                }
            };

            // Act
            this.portfolioValuationService.UpdatePortfolioTotals(userPortfolioInstance);

            // Assert
            Assert.Equal(1700m, userPortfolioInstance.TotalValue);
            Assert.Equal(450m, userPortfolioInstance.TotalGainLoss);
            Assert.Equal(36m, userPortfolioInstance.GainLossPercent);
        }
    }
}