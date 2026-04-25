using System.Collections.Generic;
using KarmaBanking.App.Models;
using KarmaBanking.App.Models.DTOs;
using KarmaBanking.App.Services;
using Xunit;

namespace KarmaBanking.App.Tests.Services;

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
    public void ValidateWithdrawRequest_ReturnsExpectedTuple(
        double amountDouble,
        bool hasDestination,
        bool expectedValid,
        string expectedError)
    {
        decimal amount = (decimal)amountDouble;
        var destination = hasDestination ? new FundingSourceOption() : null;

        var (isValid, errorMessage) = this.savingsWorkflowService.ValidateWithdrawRequest(amount, destination);

        Assert.Equal(expectedValid, isValid);
        Assert.Equal(expectedError, errorMessage);
    }

    [Fact]
    public void BuildWithdrawResultMessage_NotSuccessful_ReturnsMessage()
    {
        var response = new WithdrawResponseDto
        {
            Success = false,
            Message = "Insufficient funds."
        };

        var withdrawalMessage = this.savingsWorkflowService.BuildWithdrawResultMessage(response);

        Assert.Equal("Insufficient funds.", withdrawalMessage);
    }

    [Fact]
    public void BuildWithdrawResultMessage_SuccessWithoutPenalty_FormatsProperly()
    {
        var response = new WithdrawResponseDto
        {
            Success = true,
            AmountWithdrawn = 500m,
            PenaltyApplied = 0m,
            NewBalance = 1500m
        };
        string expected = $"Withdrawn: ${500m:N2}. New balance: ${1500m:N2}";

        var withdrawalMessage = this.savingsWorkflowService.BuildWithdrawResultMessage(response);

        Assert.Equal(expected, withdrawalMessage);
    }

    [Fact]
    public void BuildWithdrawResultMessage_SuccessWithPenalty_FormatsProperly()
    {
        var response = new WithdrawResponseDto
        {
            Success = true,
            AmountWithdrawn = 500m,
            PenaltyApplied = 25.50m,
            NewBalance = 1474.50m
        };
        string expected = $"Withdrawn: ${500m:N2} (penalty: ${25.50m:N2}). New balance: ${1474.50m:N2}";

        var withdrawalMessage = this.savingsWorkflowService.BuildWithdrawResultMessage(response);

        Assert.Equal(expected, withdrawalMessage);
    }

    [Theory]
    [InlineData(false, 1, false, "Please confirm account closure.")]
    [InlineData(true, 0, false, "Please select a destination account.")]
    [InlineData(true, 42, true, "")]
    public void ValidateCloseConfirmation_ReturnsExpectedTuple(
        bool userConfirmed,
        int destinationId,
        bool expectedValid,
        string expectedError)
    {
        var (isValid, errorMessage) = this.savingsWorkflowService.ValidateCloseConfirmation(userConfirmed, destinationId);

        Assert.Equal(expectedValid, isValid);
        Assert.Equal(expectedError, errorMessage);
    }

    [Theory]
    [InlineData(1, 5, true)]
    [InlineData(5, 5, false)]
    [InlineData(6, 5, false)]
    public void CanMoveToNextPage_ReturnsExpectedResult(int current, int total, bool expected)
    {
        Assert.Equal(expected, this.savingsWorkflowService.CanMoveToNextPage(current, total));
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(5, true)]
    public void CanMoveToPreviousPage_ReturnsExpectedResult(int current, bool expected)
    {
        Assert.Equal(expected, this.savingsWorkflowService.CanMoveToPreviousPage(current));
    }
}