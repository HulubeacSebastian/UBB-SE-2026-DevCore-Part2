using System;
using System.Collections.Generic;
using KarmaBanking.App.Models;
using KarmaBanking.App.Models.Enums;
using KarmaBanking.App.Services;
using Xunit;

namespace KarmaBanking.App.Tests.Services;

public class SavingsUiRulesServiceTests
{
    private readonly SavingsUiRulesService savingsUiRulesService;

    public SavingsUiRulesServiceTests()
    {
        this.savingsUiRulesService = new SavingsUiRulesService();
    }

    [Theory]
    [InlineData("150.75", true, 150.75)]
    [InlineData("0", false, 0)]
    [InlineData("-50", false, 0)]
    [InlineData("invalid", false, 0)]
    [InlineData(null, false, 0)]
    public void TryParsePositiveAmount_ReturnsExpectedResult(string input, bool expectedSuccess, double expectedAmountDouble)
    {
        var sucessfullyParsed = this.savingsUiRulesService.TryParsePositiveAmount(input, out var amount);

        Assert.Equal(expectedSuccess, sucessfullyParsed);
        Assert.Equal((decimal)expectedAmountDouble, amount);
    }

    [Fact]
    public void BuildDepositPreview_NullAccount_ReturnsEmpty()
    {
        var parsedDepositAmountString = this.savingsUiRulesService.BuildDepositPreview("100", null);

        Assert.Empty(parsedDepositAmountString);
    }

    [Fact]
    public void BuildDepositPreview_InvalidAmount_ReturnsEmpty()
    {
        var account = new SavingsAccount { Balance = 500m };

        var parsedDepositAmountString = this.savingsUiRulesService.BuildDepositPreview("-50", account);

        Assert.Empty(parsedDepositAmountString);
    }

    [Fact]
    public void BuildDepositPreview_ValidInput_ReturnsFormattedString()
    {
        var account = new SavingsAccount { Balance = 500m };
        string expected = $"New balance will be: ${650.50m:N2}";

        var parsedDepositAmountString = this.savingsUiRulesService.BuildDepositPreview("150.50", account);

        Assert.Equal(expected, parsedDepositAmountString);
    }

    [Fact]
    public void ValidateCreateAccount_AllValid_NonGoal_ReturnsEmptyDictionary()
    {
        var errors = this.savingsUiRulesService.ValidateCreateAccount(
            selectedSavingsType: "Standard",
            accountName: "My Savings",
            initialDepositText: "100.00",
            hasFundingSource: true,
            selectedFrequency: "Monthly",
            targetAmount: null,
            targetDate: null,
            isGoalSavings: false);

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateCreateAccount_AllValid_Goal_ReturnsEmptyDictionary()
    {
        var errors = this.savingsUiRulesService.ValidateCreateAccount(
            selectedSavingsType: "Goal",
            accountName: "Vacation",
            initialDepositText: "100.00",
            hasFundingSource: true,
            selectedFrequency: "Weekly",
            targetAmount: 5000m,
            targetDate: DateTimeOffset.UtcNow.AddDays(10), // Future date
            isGoalSavings: true);

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateCreateAccount_MissingBaseFields_ReturnsErrors()
    {
        var errors = this.savingsUiRulesService.ValidateCreateAccount(
            selectedSavingsType: " ",
            accountName: "   ",
            initialDepositText: "invalid",
            hasFundingSource: false,
            selectedFrequency: null,
            targetAmount: null,
            targetDate: null,
            isGoalSavings: false);

        Assert.Equal(5, errors.Count);
        Assert.Contains("SavingsType", errors.Keys);
        Assert.Contains("AccountName", errors.Keys);
        Assert.Contains("InitialDeposit", errors.Keys);
        Assert.Contains("FundingSource", errors.Keys);
        Assert.Contains("Frequency", errors.Keys);
    }

    [Theory]
    [InlineData(-100.0, null, 2)]
    [InlineData(null, 0, 2)]
    [InlineData(null, -5, 2)]
    public void ValidateCreateAccount_InvalidGoalFields_ReturnsErrors(double? targetAmount, int? daysToAdd, int expectedErrorCount)
    {
        DateTimeOffset? targetDate = daysToAdd.HasValue ? DateTimeOffset.Now.AddDays(daysToAdd.Value) : null;

        var errors = this.savingsUiRulesService.ValidateCreateAccount(
            selectedSavingsType: "Goal",
            accountName: "Vacation",
            initialDepositText: "100.00",
            hasFundingSource: true,
            selectedFrequency: "Weekly",
            targetAmount: targetAmount.HasValue ? (decimal)targetAmount.Value : null,
            targetDate: targetDate,
            isGoalSavings: true);

        Assert.Equal(expectedErrorCount, errors.Count);
        Assert.Contains("TargetAmount", errors.Keys);
        Assert.Contains("TargetDate", errors.Keys);
    }
}