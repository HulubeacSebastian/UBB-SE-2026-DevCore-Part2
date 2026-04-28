// <copyright file="SavingsUiRulesServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using KarmaBanking.App.Models;
    using KarmaBanking.App.Services;
    using Xunit;

    public class SavingsUiRulesServiceTests
    {
        private readonly SavingsUiRulesService savingsUiRulesService;

        public SavingsUiRulesServiceTests()
        {
            this.savingsUiRulesService = new SavingsUiRulesService();
        }

        [Theory]
        [InlineData("150.75", "150.75")]
        [InlineData("0.01", "0.01")]
        public void TryParsePositiveAmount_ValidInput_ReturnsTrueAndParsedValue(
            string amountInputText,
            string expectedAmountText)
        {
            // Arrange
            decimal expectedParsedAmount = decimal.Parse(expectedAmountText, CultureInfo.InvariantCulture);

            // Act
            bool actualParsingSuccess = this.savingsUiRulesService.TryParsePositiveAmount(
                amountInputText,
                out decimal actualParsedAmount);

            // Assert
            Assert.True(actualParsingSuccess);
            Assert.Equal(expectedParsedAmount, actualParsedAmount);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-50")]
        [InlineData("invalid")]
        [InlineData(null)]
        [InlineData("")]
        public void TryParsePositiveAmount_InvalidOrNonPositiveInput_ReturnsFalse(
            string amountInputText)
        {
            // Act
            bool actualParsingSuccess = this.savingsUiRulesService.TryParsePositiveAmount(
                amountInputText,
                out decimal actualParsedAmount);

            // Assert
            Assert.False(actualParsingSuccess);
            Assert.Equal(0m, actualParsedAmount);
        }

        [Fact]
        public void BuildDepositPreview_NullAccount_ReturnsEmpty()
        {
            // Act
            string actualDepositPreviewMessage = this.savingsUiRulesService.BuildDepositPreview("100", null);

            // Assert
            Assert.Empty(actualDepositPreviewMessage);
        }

        [Fact]
        public void BuildDepositPreview_InvalidAmount_ReturnsEmpty()
        {
            // Arrange
            var savingsAccountInstance = new SavingsAccount { Balance = 500m };

            // Act
            string actualDepositPreviewMessage = this.savingsUiRulesService.BuildDepositPreview("-50", savingsAccountInstance);

            // Assert
            Assert.Empty(actualDepositPreviewMessage);
        }

        [Fact]
        public void BuildDepositPreview_ValidInput_ReturnsFormattedString()
        {
            // Arrange
            var savingsAccountInstance = new SavingsAccount { Balance = 500m };
            string expectedPreviewMessage = $"New balance will be: ${650.50m:N2}";

            // Act
            string actualDepositPreviewMessage = this.savingsUiRulesService.BuildDepositPreview("150.50", savingsAccountInstance);

            // Assert
            Assert.Equal(expectedPreviewMessage, actualDepositPreviewMessage);
        }

        [Fact]
        public void ValidateCreateAccount_AllValid_NonGoal_ReturnsEmptyDictionary()
        {
            // Act
            var validationErrorDictionary = this.savingsUiRulesService.ValidateCreateAccount(
                selectedSavingsType: "Standard",
                accountName: "My Savings",
                initialDepositText: "100.00",
                hasFundingSource: true,
                selectedFrequency: "Monthly",
                targetAmount: null,
                targetDate: null,
                isGoalSavings: false);

            // Assert
            Assert.Empty(validationErrorDictionary);
        }

        [Fact]
        public void ValidateCreateAccount_AllValid_Goal_ReturnsEmptyDictionary()
        {
            // Act
            var validationErrorDictionary = this.savingsUiRulesService.ValidateCreateAccount(
                selectedSavingsType: "Goal",
                accountName: "Vacation",
                initialDepositText: "100.00",
                hasFundingSource: true,
                selectedFrequency: "Weekly",
                targetAmount: 5000m,
                targetDate: DateTimeOffset.UtcNow.AddDays(10),
                isGoalSavings: true);

            // Assert
            Assert.Empty(validationErrorDictionary);
        }

        [Fact]
        public void ValidateCreateAccount_MissingBaseFields_ReturnsErrors()
        {
            // Act
            var validationErrorDictionary = this.savingsUiRulesService.ValidateCreateAccount(
                selectedSavingsType: " ",
                accountName: "   ",
                initialDepositText: "invalid",
                hasFundingSource: false,
                selectedFrequency: null,
                targetAmount: null,
                targetDate: null,
                isGoalSavings: false);

            // Assert
            Assert.Equal(5, validationErrorDictionary.Count);
            Assert.Contains("SavingsType", validationErrorDictionary.Keys);
            Assert.Contains("AccountName", validationErrorDictionary.Keys);
        }

        [Theory]
        [InlineData(-100.0, null, 2)]
        [InlineData(null, 0, 2)]
        [InlineData(null, -5, 2)]
        public void ValidateCreateAccount_InvalidGoalFields_ReturnsErrors(
            double? targetAmountValue,
            int? daysToAddToTargetDate,
            int expectedErrorCount)
        {
            // Arrange
            DateTimeOffset? targetDateValue = daysToAddToTargetDate.HasValue
                ? DateTimeOffset.Now.AddDays(daysToAddToTargetDate.Value)
                : null;

            // Act
            var validationErrorDictionary = this.savingsUiRulesService.ValidateCreateAccount(
                selectedSavingsType: "Goal",
                accountName: "Vacation",
                initialDepositText: "100.00",
                hasFundingSource: true,
                selectedFrequency: "Weekly",
                targetAmount: targetAmountValue.HasValue ? (decimal)targetAmountValue.Value : null,
                targetDate: targetDateValue,
                isGoalSavings: true);

            // Assert
            Assert.Equal(expectedErrorCount, validationErrorDictionary.Count);
            Assert.Contains("TargetAmount", validationErrorDictionary.Keys);
            Assert.Contains("TargetDate", validationErrorDictionary.Keys);
        }
    }
}