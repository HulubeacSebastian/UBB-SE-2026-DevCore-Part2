// <copyright file="SavingsPresentationServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using System.Collections.Generic;
    using KarmaBanking.App.Models;
    using KarmaBanking.App.Services;
    using Xunit;

    public class SavingsPresentationServiceTests
    {
        private const decimal FirstAccountBalance = 1500.50m;
        private const decimal SecondAccountBalance = 2500.25m;
        private const decimal ExpectedTotalBalance = FirstAccountBalance + SecondAccountBalance;

        private const int ZeroAccounts = 0;
        private const int SingleAccount = 1;
        private const int MultipleAccounts = 2;

        public static IEnumerable<object[]> AccountCountCases =>
        [
            [ZeroAccounts, "across 0 accounts"],
            [SingleAccount, "across 1 account"],
            [MultipleAccounts, "across 2 accounts"],
        ];

        private readonly SavingsPresentationService savingsPresentationService;

        public SavingsPresentationServiceTests()
        {
            this.savingsPresentationService = new SavingsPresentationService();
        }

        [Fact]
        public void BuildTotalSavedAmount_WhenMultipleAccounts_ThenCalculatesSumAndFormatsProperly()
        {
            // Arrange
            var savingsAccountsList = new List<SavingsAccount>
            {
                new SavingsAccount { Balance = FirstAccountBalance },
                new SavingsAccount { Balance = SecondAccountBalance }
            };
            string expectedFormattedBalanceText = $"${ExpectedTotalBalance:F2}";

            // Act
            string actualFormattedBalanceResult = this.savingsPresentationService.BuildTotalSavedAmount(savingsAccountsList);

            // Assert
            Assert.Equal(expectedFormattedBalanceText, actualFormattedBalanceResult);
        }

        [Theory]
        [MemberData(nameof(AccountCountCases))]
        public void BuildNumberOfAccountsText_WhenGivenVariousAccountCounts_ThenHandlesPluralization(int totalAccountsCount, string expectedPluralizedText)
        {
            // Act
            string actualPluralizedResultText = this.savingsPresentationService.BuildNumberOfAccountsText(totalAccountsCount);

            // Assert
            Assert.Equal(expectedPluralizedText, actualPluralizedResultText);
        }
    }
}