// <copyright file="SavingsWorkflowServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using System.Collections.Generic;
    using KarmaBanking.App.Models;
    using KarmaBanking.App.Models.DTOs;
    using KarmaBanking.App.Services;
    using Xunit;

    public class SavingsWorkflowServiceTests
    {
        private readonly SavingsWorkflowService savingsWorkflowService;

        public SavingsWorkflowServiceTests()
        {
            this.savingsWorkflowService = new SavingsWorkflowService();
        }

        [Theory]
        [InlineData(-50.0, true, false, "Please enter a valid amount.")]
        [InlineData(0.0, true, false, "Please enter a valid amount.")]
        [InlineData(100.0, false, false, "Please select a destination account.")]
        [InlineData(100.0, true, true, "")]
        public void ValidateWithdrawRequest_WhenGivenVariousInputs_ThenReturnsExpectedTuple(
            double withdrawalAmountValue,
            bool isDestinationSelected,
            bool isExpectedToBeValid,
            string expectedValidationErrorMessage)
        {
            // Arrange
            decimal withdrawalAmount = (decimal)withdrawalAmountValue;
            var fundingSourceOptionInstance = isDestinationSelected ? new FundingSourceOption() : null;

            // Act
            var (isRequestValid, actualErrorMessage) = this.savingsWorkflowService.ValidateWithdrawRequest(
                withdrawalAmount,
                fundingSourceOptionInstance);

            // Assert
            Assert.Equal(isExpectedToBeValid, isRequestValid);
            Assert.Equal(expectedValidationErrorMessage, actualErrorMessage);
        }

        [Fact]
        public void BuildWithdrawResultMessage_WhenWithdrawNotSuccessful_ThenReturnsErrorMessage()
        {
            // Arrange
            var withdrawResponseDataTransferObject = new WithdrawResponseDto
            {
                Success = false,
                Message = "Insufficient funds."
            };

            // Act
            string actualWithdrawalResultMessage = this.savingsWorkflowService.BuildWithdrawResultMessage(withdrawResponseDataTransferObject);

            // Assert
            Assert.Equal("Insufficient funds.", actualWithdrawalResultMessage);
        }

        [Fact]
        public void BuildWithdrawResultMessage_WhenSuccessWithoutPenalty_ThenFormatsProperly()
        {
            // Arrange
            var withdrawResponseDataTransferObject = new WithdrawResponseDto
            {
                Success = true,
                AmountWithdrawn = 500m,
                PenaltyApplied = 0m,
                NewBalance = 1500m
            };
            string expectedResultMessage = $"Withdrawn: ${500m:N2}. New balance: ${1500m:N2}";

            // Act
            string actualWithdrawalResultMessage = this.savingsWorkflowService.BuildWithdrawResultMessage(withdrawResponseDataTransferObject);

            // Assert
            Assert.Equal(expectedResultMessage, actualWithdrawalResultMessage);
        }

        [Fact]
        public void BuildWithdrawResultMessage_WhenSuccessWithPenalty_ThenFormatsProperly()
        {
            // Arrange
            var withdrawResponseDataTransferObject = new WithdrawResponseDto
            {
                Success = true,
                AmountWithdrawn = 500m,
                PenaltyApplied = 25.50m,
                NewBalance = 1474.50m
            };
            string expectedResultMessage = $"Withdrawn: ${500m:N2} (penalty: ${25.50m:N2}). New balance: ${1474.50m:N2}";

            // Act
            string actualWithdrawalResultMessage = this.savingsWorkflowService.BuildWithdrawResultMessage(withdrawResponseDataTransferObject);

            // Assert
            Assert.Equal(expectedResultMessage, actualWithdrawalResultMessage);
        }

        [Theory]
        [InlineData(false, 1, false, "Please confirm account closure.")]
        [InlineData(true, 0, false, "Please select a destination account.")]
        [InlineData(true, 42, true, "")]
        public void ValidateCloseConfirmation_WhenGivenVariousInputs_ThenReturnsExpectedTuple(
            bool isUserConfirmationProvided,
            int destinationIdentificationNumber,
            bool isExpectedToBeValid,
            string expectedValidationErrorMessage)
        {
            // Act
            var (isConfirmationValid, actualErrorMessage) = this.savingsWorkflowService.ValidateCloseConfirmation(
                isUserConfirmationProvided,
                destinationIdentificationNumber);

            // Assert
            Assert.Equal(isExpectedToBeValid, isConfirmationValid);
            Assert.Equal(expectedValidationErrorMessage, actualErrorMessage);
        }

        [Theory]
        [InlineData(1, 5, true)]
        [InlineData(5, 5, false)]
        [InlineData(6, 5, false)]
        public void CanMoveToNextPage_WhenGivenVariousPageIndices_ThenReturnsExpectedResult(int currentPageIndex, int totalPageCount, bool isExpectedToMove)
        {
            // Act
            bool actualCanMoveResult = this.savingsWorkflowService.CanMoveToNextPage(currentPageIndex, totalPageCount);

            // Assert
            Assert.Equal(isExpectedToMove, actualCanMoveResult);
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(2, true)]
        [InlineData(5, true)]
        public void CanMoveToPreviousPage_WhenGivenVariousPageIndices_ThenReturnsExpectedResult(int currentPageIndex, bool isExpectedToMove)
        {
            // Act
            bool actualCanMoveResult = this.savingsWorkflowService.CanMoveToPreviousPage(currentPageIndex);

            // Assert
            Assert.Equal(isExpectedToMove, actualCanMoveResult);
        }
    }
}