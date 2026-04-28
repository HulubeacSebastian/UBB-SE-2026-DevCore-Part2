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

        [Fact]
        public void ValidateWithdrawRequest_ValidAmountAndDestination_ReturnsTrueAndEmptyError()
        {
            // Arrange
            decimal validAmount = 100.00m;
            var destinationAccount = new FundingSourceOption();

            // Act
            var (isRequestValid, actualErrorMessage) = this.savingsWorkflowService.ValidateWithdrawRequest(
                validAmount,
                destinationAccount);

            // Assert
            Assert.True(isRequestValid);
            Assert.Empty(actualErrorMessage);
        }

        [Theory]
        [InlineData("-50")]
        [InlineData("0")]
        public void ValidateWithdrawRequest_InvalidAmount_ReturnsFalseAndAmountError(string invalidAmountStr)
        {
            // Arrange
            decimal invalidAmount = decimal.Parse(invalidAmountStr);
            var validDestinationAccount = new FundingSourceOption();

            // Act
            var (isRequestValid, actualErrorMessage) = this.savingsWorkflowService.ValidateWithdrawRequest(
                invalidAmount,
                validDestinationAccount);

            // Assert
            Assert.False(isRequestValid);
            Assert.Equal("Please enter a valid amount.", actualErrorMessage);
        }

        [Fact]
        public void ValidateWithdrawRequest_NullDestination_ReturnsFalseAndDestinationError()
        {
            // Arrange
            decimal validAmount = 100.00m;
            FundingSourceOption nullDestinationAccount = null;

            // Act
            var (isRequestValid, actualErrorMessage) = this.savingsWorkflowService.ValidateWithdrawRequest(
                validAmount,
                nullDestinationAccount);

            // Assert
            Assert.False(isRequestValid);
            Assert.Equal("Please select a destination account.", actualErrorMessage);
        }

        [Fact]
        public void BuildWithdrawResultMessage_NotSuccessful_ReturnsMessage()
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
        public void BuildWithdrawResultMessage_SuccessWithoutPenalty_FormatsProperly()
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
        public void BuildWithdrawResultMessage_SuccessWithPenalty_FormatsProperly()
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

        [Fact]
        public void ValidateCloseConfirmation_ValidInputs_ReturnsTrueAndEmptyError()
        {
            // Arrange
            bool userConfirmed = true;
            int validDestinationId = 42;

            // Act
            var (isConfirmationValid, actualErrorMessage) = this.savingsWorkflowService.ValidateCloseConfirmation(
                userConfirmed,
                validDestinationId);

            // Assert
            Assert.True(isConfirmationValid);
            Assert.Empty(actualErrorMessage);
        }

        [Fact]
        public void ValidateCloseConfirmation_MissingConfirmation_ReturnsFalseAndConfirmationError()
        {
            // Arrange
            bool userConfirmed = false;
            int validDestinationId = 42;

            // Act
            var (isConfirmationValid, actualErrorMessage) = this.savingsWorkflowService.ValidateCloseConfirmation(
                userConfirmed,
                validDestinationId);

            // Assert
            Assert.False(isConfirmationValid);
            Assert.Equal("Please confirm account closure.", actualErrorMessage);
        }

        [Theory]
        [InlineData(0)]
        public void ValidateCloseConfirmation_InvalidDestination_ReturnsFalseAndDestinationError(int invalidDestinationId)
        {
            // Arrange
            bool userConfirmed = true;

            // Act
            var (isConfirmationValid, actualErrorMessage) = this.savingsWorkflowService.ValidateCloseConfirmation(
                userConfirmed,
                invalidDestinationId);

            // Assert
            Assert.False(isConfirmationValid);
            Assert.Equal("Please select a destination account.", actualErrorMessage);
        }

        [Theory]
        [InlineData(1, 5)]
        [InlineData(4, 5)]
        public void CanMoveToNextPage_NotOnLastPage_ReturnsTrue(int currentPageIndex, int totalPageCount)
        {
            // Act
            bool actualCanMoveResult = this.savingsWorkflowService.CanMoveToNextPage(currentPageIndex, totalPageCount);

            // Assert
            Assert.True(actualCanMoveResult);
        }

        [Theory]
        [InlineData(5, 5)]
        [InlineData(6, 5)]
        public void CanMoveToNextPage_OnOrPastLastPage_ReturnsFalse(int currentPageIndex, int totalPageCount)
        {
            // Act
            bool actualCanMoveResult = this.savingsWorkflowService.CanMoveToNextPage(currentPageIndex, totalPageCount);

            // Assert
            Assert.False(actualCanMoveResult);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        public void CanMoveToPreviousPage_PageIsGreaterThanOne_ReturnsTrue(int currentPageIndex)
        {
            // Act
            bool actualCanMoveResult = this.savingsWorkflowService.CanMoveToPreviousPage(currentPageIndex);

            // Assert
            Assert.True(actualCanMoveResult);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-1)]
        public void CanMoveToPreviousPage_OnFirstPageOrLower_ReturnsFalse(int currentPageIndex)
        {
            // Act
            bool actualCanMoveResult = this.savingsWorkflowService.CanMoveToPreviousPage(currentPageIndex);

            // Assert
            Assert.False(actualCanMoveResult);
        }
    }
}