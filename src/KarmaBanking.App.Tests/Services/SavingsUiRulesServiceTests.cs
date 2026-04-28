namespace KarmaBanking.App.Tests.Services
{
    using System;
    using System.Collections.Generic;
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
        [InlineData("150.75", true, 150.75)]
        [InlineData("0", false, 0)]
        [InlineData("-50", false, 0)]
        [InlineData("invalid", false, 0)]
        [InlineData(null, false, 0)]
        public void TryParsePositiveAmount_WhenGivenVariousInputs_ThenReturnsExpectedResult(
            string amountInputText,
            bool isExpectedToParseSuccessfully,
            double expectedParsedAmountValue)
        {
            // Act
            bool actualParsingSuccess = this.savingsUiRulesService.TryParsePositiveAmount(
                amountInputText,
                out decimal actualParsedAmount);

            // Assert
            Assert.Equal(isExpectedToParseSuccessfully, actualParsingSuccess);
            Assert.Equal((decimal)expectedParsedAmountValue, actualParsedAmount);
        }

        [Fact]
        public void BuildDepositPreview_WhenAccountIsNull_ThenReturnsEmpty()
        {
            // Act
            string actualDepositPreviewMessage = this.savingsUiRulesService.BuildDepositPreview("100", null);

            // Assert
            Assert.Empty(actualDepositPreviewMessage);
        }

        [Fact]
        public void BuildDepositPreview_WhenAmountIsInvalid_ThenReturnsEmpty()
        {
            // Arrange
            var savingsAccountInstance = new SavingsAccount { Balance = 500m };

            // Act
            string actualDepositPreviewMessage = this.savingsUiRulesService.BuildDepositPreview("-50", savingsAccountInstance);

            // Assert
            Assert.Empty(actualDepositPreviewMessage);
        }

        [Fact]
        public void BuildDepositPreview_WhenValidInputProvided_ThenReturnsFormattedString()
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
        public void ValidateCreateAccount_WhenAllValidNonGoalSavings_ThenReturnsEmptyDictionary()
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
        public void ValidateCreateAccount_WhenAllValidGoalSavings_ThenReturnsEmptyDictionary()
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
        public void ValidateCreateAccount_WhenMissingBaseFields_ThenReturnsErrors()
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
        public void ValidateCreateAccount_WhenInvalidGoalFields_ThenReturnsErrors(
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
