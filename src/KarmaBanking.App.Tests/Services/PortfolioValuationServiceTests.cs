using System.Collections.Generic;
using KarmaBanking.App.Models;
using KarmaBanking.App.Services;
using Xunit;

namespace KarmaBanking.App.Tests.Services;

public class PortfolioValuationServiceTests
{
    private readonly PortfolioValuationService portofolioValuator;

    public PortfolioValuationServiceTests()
    {
        this.portofolioValuator = new PortfolioValuationService();
    }

    [Fact]
    public void UpdateHoldingValuation_UpdatesPriceAndCalculatesGainLoss()
    {
        var holding = new InvestmentHolding
        {
            AveragePurchasePrice = 50m,
            Quantity = 10m
        };
        decimal newPrice = 75m;

        this.portofolioValuator.UpdateHoldingValuation(holding, newPrice);

        Assert.Equal(75m, holding.CurrentPrice);
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

        this.portofolioValuator.UpdateHoldingValuation(holding, newPrice);

        Assert.Equal(80m, holding.CurrentPrice);
        Assert.Equal(-100m, holding.UnrealizedGainLoss);
    }

    [Fact]
    public void UpdatePortfolioTotals_WithPositiveTotalCost_CalculatesTotalsCorrectly()
    {
        var portfolio = new Portfolio
        {
            Holdings = new List<InvestmentHolding>
            {
                new InvestmentHolding { Quantity = 10m, AveragePurchasePrice = 100m, CurrentPrice = 150m, UnrealizedGainLoss = 500m },
                new InvestmentHolding { Quantity = 5m, AveragePurchasePrice = 50m, CurrentPrice = 40m, UnrealizedGainLoss = -50m }
            }
        };
        this.portofolioValuator.UpdatePortfolioTotals(portfolio);

        Assert.Equal(1700m, portfolio.TotalValue);

        Assert.Equal(450m, portfolio.TotalGainLoss);

        Assert.Equal(36m, portfolio.GainLossPercent);
    }

    [Fact]
    public void UpdatePortfolioTotals_WithZeroTotalCost_AvoidsDivideByZero()
    {
        var portfolio = new Portfolio
        {
            Holdings = new List<InvestmentHolding>
            {
                new InvestmentHolding { Quantity = 0m, AveragePurchasePrice = 0m, CurrentPrice = 100m, UnrealizedGainLoss = 0m }
            }
        };

        this.portofolioValuator.UpdatePortfolioTotals(portfolio);

        Assert.Equal(0m, portfolio.TotalValue);
        Assert.Equal(0m, portfolio.TotalGainLoss);

        Assert.Equal(0m, portfolio.GainLossPercent);
    }

    [Fact]
    public void UpdatePortfolioTotals_WithEmptyHoldings_CalculatesZeroes()
    {
        var portfolio = new Portfolio
        {
            Holdings = new List<InvestmentHolding>()
        };

        this.portofolioValuator.UpdatePortfolioTotals(portfolio);

        Assert.Equal(0m, portfolio.TotalValue);
        Assert.Equal(0m, portfolio.TotalGainLoss);
        Assert.Equal(0m, portfolio.GainLossPercent);
    }
}