// <copyright file="PortfolioValuationService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;
using KarmaBanking.App.Models;
using KarmaBanking.App.Services;
using Xunit;

namespace KarmaBanking.App.Tests.Services;

public class PortfolioValuationServiceTests
{
    private readonly PortfolioValuationService service;

    public PortfolioValuationServiceTests()
    {
        this.service = new PortfolioValuationService();
    }

    // UpdateHoldingValuation Tests
    [Fact]
    public void UpdateHoldingValuation_UpdatesPriceAndCalculatesGainLoss()
    {
        var holding = new InvestmentHolding
        {
            AveragePurchasePrice = 50m,
            Quantity = 10m
        };
        decimal newPrice = 75m;

        this.service.UpdateHoldingValuation(holding, newPrice);

        Assert.Equal(75m, holding.CurrentPrice);
        // Expected UnrealizedGainLoss: (CurrentPrice 75 - AvgPrice 50) * Qty 10 = 250
        Assert.Equal(250m, holding.UnrealizedGainLoss);
    }

    [Fact]
    public void UpdateHoldingValuation_WithNegativeMovement_CalculatesLoss()
    {
        var holding = new InvestmentHolding
        {
            AveragePurchasePrice = 100m,
            Quantity = 5m
        };
        decimal newPrice = 80m;

        this.service.UpdateHoldingValuation(holding, newPrice);

        Assert.Equal(80m, holding.CurrentPrice);
        // Expected UnrealizedGainLoss: (CurrentPrice 80 - AvgPrice 100) * Qty 5 = -100
        Assert.Equal(-100m, holding.UnrealizedGainLoss);
    }

    // UpdatePortfolioTotals Tests
    [Fact]
    public void UpdatePortfolioTotals_WithPositiveTotalCost_CalculatesTotalsCorrectly()
    {
        var portfolio = new Portfolio
        {
            Holdings = new List<InvestmentHolding>
            {
                // Cost: 10 * 100 = 1000. Value: 10 * 150 = 1500
                new InvestmentHolding { Quantity = 10m, AveragePurchasePrice = 100m, CurrentPrice = 150m, UnrealizedGainLoss = 500m },
                // Cost: 5 * 50 = 250. Value: 5 * 40 = 200
                new InvestmentHolding { Quantity = 5m, AveragePurchasePrice = 50m, CurrentPrice = 40m, UnrealizedGainLoss = -50m }
            }
        };
        this.service.UpdatePortfolioTotals(portfolio);

        // Total Value = 1500 + 200 = 1700
        Assert.Equal(1700m, portfolio.TotalValue);

        // Total GainLoss = 500 - 50 = 450
        Assert.Equal(450m, portfolio.TotalGainLoss);

        // Total Cost = 1000 + 250 = 1250
        // Percent = (450 / 1250) * 100 = 36%
        Assert.Equal(36m, portfolio.GainLossPercent);
    }

    [Fact]
    public void UpdatePortfolioTotals_WithZeroTotalCost_AvoidsDivideByZero()
    {
        var portfolio = new Portfolio
        {
            Holdings = new List<InvestmentHolding>
            {
                // Zero cost setup to trigger the false path of the ternary operator
                new InvestmentHolding { Quantity = 0m, AveragePurchasePrice = 0m, CurrentPrice = 100m, UnrealizedGainLoss = 0m }
            }
        };

        this.service.UpdatePortfolioTotals(portfolio);

        Assert.Equal(0m, portfolio.TotalValue);
        Assert.Equal(0m, portfolio.TotalGainLoss);

        // Should be exactly 0 due to the ternary operator catching the 0 totalCost
        Assert.Equal(0m, portfolio.GainLossPercent);
    }

    [Fact]
    public void UpdatePortfolioTotals_WithEmptyHoldings_CalculatesZeroes()
    {
        var portfolio = new Portfolio
        {
            Holdings = new List<InvestmentHolding>()
        };

        this.service.UpdatePortfolioTotals(portfolio);

        Assert.Equal(0m, portfolio.TotalValue);
        Assert.Equal(0m, portfolio.TotalGainLoss);
        Assert.Equal(0m, portfolio.GainLossPercent);
    }
}