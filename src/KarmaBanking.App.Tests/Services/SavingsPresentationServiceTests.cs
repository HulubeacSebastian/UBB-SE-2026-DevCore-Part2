// <copyright file="SavingsPresentationServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using KarmaBanking.App.Models;
using KarmaBanking.App.Services;
using Xunit;

namespace KarmaBanking.App.Tests.Services;

public class SavingsPresentationServiceTests
{
    private readonly SavingsPresentationService service;

    public SavingsPresentationServiceTests()
    {
        this.service = new SavingsPresentationService();
    }

    // BuildTotalSavedAmount Tests
    [Fact]
    public void BuildTotalSavedAmount_CalculatesSumAndFormatsProperly()
    {
        var accounts = new List<SavingsAccount>
        {
            new SavingsAccount { Balance = 1500.50m },
            new SavingsAccount { Balance = 2500.25m }
        };

        // Sum is 4000.75. We format it dynamically using the same mask to avoid OS culture mismatch issues (e.g. '.' vs ',')
        string expected = $"${4000.75m:F2}";

        var result = this.service.BuildTotalSavedAmount(accounts);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void BuildTotalSavedAmount_EmptyList_ReturnsZeroFormatted()
    {
        var accounts = new List<SavingsAccount>();
        string expected = $"${0m:F2}";

        var result = this.service.BuildTotalSavedAmount(accounts);

        Assert.Equal(expected, result);
    }

    // BuildNumberOfAccountsText Tests
    [Theory]
    [InlineData(0, "across 0 accounts")]
    [InlineData(1, "across 1 account")]
    [InlineData(2, "across 2 accounts")]
    [InlineData(10, "across 10 accounts")]
    public void BuildNumberOfAccountsText_HandlesPluralization(int count, string expected)
    {
        var result = this.service.BuildNumberOfAccountsText(count);

        Assert.Equal(expected, result);
    }

    // BuildBestInterestRate Tests
    [Fact]
    public void BuildBestInterestRate_WithAccounts_ReturnsMaxApyFormatted()
    {
        var accounts = new List<SavingsAccount>
        {
            new SavingsAccount { Apy = 0.015m }, // 1.5%
            new SavingsAccount { Apy = 0.0525m }, // 5.25% - Best
            new SavingsAccount { Apy = 0.03m } // 3.0%
        };

        // Expected is 0.0525 * 100 = 5.25
        string expected = $"{5.25m:F2}%";

        var result = this.service.BuildBestInterestRate(accounts);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void BuildBestInterestRate_EmptyList_ReturnsZeroFormatted()
    {
        var accounts = new List<SavingsAccount>();
        string expected = $"{0m:F2}%";

        var result = this.service.BuildBestInterestRate(accounts);

        Assert.Equal(expected, result);
    }

    // HasClosePenaltyRisk Tests
    [Fact]
    public void HasClosePenaltyRisk_NullAccount_ReturnsFalse()
    {
        var result = this.service.HasClosePenaltyRisk(null);

        Assert.False(result);
    }

    [Fact]
    public void HasClosePenaltyRisk_NotFixedDeposit_ReturnsFalse()
    {
        var account = new SavingsAccount
        {
            SavingsType = "StandardSavings",
            MaturityDate = DateTime.UtcNow.AddDays(10) // Future date, but wrong type
        };

        var result = this.service.HasClosePenaltyRisk(account);

        Assert.False(result);
    }

    [Fact]
    public void HasClosePenaltyRisk_NullMaturityDate_ReturnsFalse()
    {
        var account = new SavingsAccount
        {
            SavingsType = "FixedDeposit",
            MaturityDate = null
        };

        var result = this.service.HasClosePenaltyRisk(account);

        Assert.False(result);
    }

    [Fact]
    public void HasClosePenaltyRisk_PastMaturityDate_ReturnsFalse()
    {
        var account = new SavingsAccount
        {
            SavingsType = "FixedDeposit",
            MaturityDate = DateTime.UtcNow.AddDays(-1) // Past date
        };

        var result = this.service.HasClosePenaltyRisk(account);

        Assert.False(result);
    }

    [Fact]
    public void HasClosePenaltyRisk_FutureMaturityDateAndFixedDeposit_ReturnsTrue()
    {
        var account = new SavingsAccount
        {
            SavingsType = "FixedDeposit",
            MaturityDate = DateTime.UtcNow.AddDays(30) // Future date
        };

        var result = this.service.HasClosePenaltyRisk(account);

        Assert.True(result);
    }
}