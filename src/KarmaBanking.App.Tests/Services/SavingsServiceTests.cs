// <copyright file="SavingsServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using KarmaBanking.App.Models;
    using KarmaBanking.App.Models.DTOs;
    using KarmaBanking.App.Models.Enums;
    using KarmaBanking.App.Repositories.Interfaces;
    using KarmaBanking.App.Services;
    using Moq;
    using Xunit;

    public class SavingsServiceTests
    {
        private const decimal FixedDepositAnnualPercentageYield = 0.04m;
        private const decimal GoalSavingsAnnualPercentageYield = 0.03m;
        private const decimal HighYieldAnnualPercentageYield = 0.03m;
        private const decimal DefaultAnnualPercentageYield = 0.02m;
        private const decimal DecimalEarlyClosurePenalty = 0.02m;
        private const decimal DecimalEarlyWithdrawalPenalty = 0.02m;

        private readonly Mock<ISavingsRepository> savingsRepositoryMock;
        private readonly SavingsService savingsService;

        public SavingsServiceTests()
        {
            this.savingsRepositoryMock = new Mock<ISavingsRepository>();
            this.savingsService = new SavingsService(this.savingsRepositoryMock.Object);
        }

        #region Create Account Tests

        [Fact]
        public async Task CreateAccountAsync_WhenStandardAccountIsCreated_ThenReturnsCreatedAccount()
        {
            // Arrange
            var inputDto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "Standard", InitialDeposit = 1000m };
            var outputAccount = new SavingsAccount { IdentificationNumber = 1, UserIdentificationNumber = 1, Balance = 1000m };

            this.savingsRepositoryMock.Setup(r => r.CreateSavingsAccountAsync(inputDto, DefaultAnnualPercentageYield)).ReturnsAsync(outputAccount);
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(new List<SavingsAccount>());

            // Act
            var result = await this.savingsService.CreateAccountAsync(inputDto);

            // Assert
            Assert.Equal(outputAccount, result);
        }

        [Fact]
        public async Task CreateAccountAsync_WhenGoalSavingsAccountIsCreated_ThenReturnsCreatedAccount()
        {
            // Arrange
            var inputDto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "GoalSavings", TargetAmount = 5000m, TargetDate = DateTime.UtcNow.AddDays(30) };
            var outputAccount = new SavingsAccount { IdentificationNumber = 1, UserIdentificationNumber = 1, TargetAmount = 5000m };

            this.savingsRepositoryMock.Setup(r => r.CreateSavingsAccountAsync(inputDto, GoalSavingsAnnualPercentageYield)).ReturnsAsync(outputAccount);
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(new List<SavingsAccount>());

            // Act
            var result = await this.savingsService.CreateAccountAsync(inputDto);

            // Assert
            Assert.Equal(outputAccount, result);
        }

        [Fact]
        public async Task CreateAccountAsync_WhenFixedDepositAccountIsCreated_ThenCallsRepositoryWithCorrectRate()
        {
            // Arrange
            var inputDto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "FixedDeposit" };
            this.savingsRepositoryMock.Setup(r => r.CreateSavingsAccountAsync(inputDto, FixedDepositAnnualPercentageYield)).ReturnsAsync(new SavingsAccount());
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(new List<SavingsAccount>());

            // Act
            await this.savingsService.CreateAccountAsync(inputDto);

            // Assert
            this.savingsRepositoryMock.Verify(r => r.CreateSavingsAccountAsync(inputDto, FixedDepositAnnualPercentageYield), Times.Once);
        }

        [Fact]
        public async Task CreateAccountAsync_WhenHighYieldAccountIsCreated_ThenCallsRepositoryWithCorrectRate()
        {
            // Arrange
            var inputDto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "HighYield" };
            this.savingsRepositoryMock.Setup(r => r.CreateSavingsAccountAsync(inputDto, HighYieldAnnualPercentageYield)).ReturnsAsync(new SavingsAccount());
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(new List<SavingsAccount>());

            // Act
            await this.savingsService.CreateAccountAsync(inputDto);

            // Assert
            this.savingsRepositoryMock.Verify(r => r.CreateSavingsAccountAsync(inputDto, HighYieldAnnualPercentageYield), Times.Once);
        }

        [Fact]
        public async Task CreateAccountAsync_WhenUserHasMaxActiveAccounts_ThenThrowsInvalidOperationException()
        {
            // Arrange
            var activeAccounts = new List<SavingsAccount> { new(), new(), new(), new(), new() };
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(activeAccounts);

            var dto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "Standard" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => this.savingsService.CreateAccountAsync(dto));
        }

        [Fact]
        public async Task CreateAccountAsync_WhenGoalSavingsWithoutTargetDate_ThenThrowsArgumentException()
        {
            // Arrange
            var dto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "GoalSavings", TargetAmount = 5000m };
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(new List<SavingsAccount>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => this.savingsService.CreateAccountAsync(dto));
        }

        [Fact]
        public async Task CreateAccountAsync_WhenGoalSavingsWithPastTargetDate_ThenThrowsArgumentException()
        {
            // Arrange
            var dto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "GoalSavings", TargetDate = DateTime.UtcNow.AddDays(-1) };
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(new List<SavingsAccount>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => this.savingsService.CreateAccountAsync(dto));
        }

        [Fact]
        public async Task CreateAccountAsync_WhenGoalSavingsWithoutTargetAmount_ThenThrowsArgumentException()
        {
            // Arrange
            var dto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "GoalSavings", TargetDate = DateTime.UtcNow.AddDays(30) };
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(new List<SavingsAccount>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => this.savingsService.CreateAccountAsync(dto));
        }

        [Fact]
        public async Task CreateAccountAsync_WhenGoalSavingsWithNegativeTargetAmount_ThenThrowsArgumentException()
        {
            // Arrange
            var dto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "GoalSavings", TargetDate = DateTime.UtcNow.AddDays(30), TargetAmount = -100m };
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(new List<SavingsAccount>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => this.savingsService.CreateAccountAsync(dto));
        }

        #endregion

        #region Deposit & Withdrawal Tests

        [Fact]
        public async Task DepositAsync_WhenAmountIsNegative_ThenThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => this.savingsService.DepositAsync(1, -10m, "Source", 1));
        }

        [Fact]
        public async Task DepositAsync_WhenAccountIdIsInvalid_ThenThrowsInvalidOperationException()
        {
            // Arrange
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, true)).ReturnsAsync(new List<SavingsAccount>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => this.savingsService.DepositAsync(99, 100m, "Source", 1));
        }

        [Fact]
        public async Task DepositAsync_WhenAccountStatusIsClosed_ThenThrowsInvalidOperationException()
        {
            // Arrange
            var account = new SavingsAccount { IdentificationNumber = 1, AccountStatus = AccountStatus.Closed.ToString() };
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, true)).ReturnsAsync(new List<SavingsAccount> { account });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => this.savingsService.DepositAsync(1, 100m, "Source", 1));
        }

        [Fact]
        public async Task DepositAsync_WhenValidDepositProvided_ThenReturnsDepositResponse()
        {
            // Arrange
            var account = new SavingsAccount { IdentificationNumber = 1, UserIdentificationNumber = 1, AccountStatus = "Active" };
            var response = new DepositResponseDto { NewBalance = 500m };
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, true)).ReturnsAsync(new List<SavingsAccount> { account });
            this.savingsRepositoryMock.Setup(r => r.DepositAsync(1, 100m, "Source")).ReturnsAsync(response);

            // Act
            var result = await this.savingsService.DepositAsync(1, 100m, "Source", 1);

            // Assert
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task WithdrawAsync_WhenInsufficientBalance_ThenThrowsInvalidOperationException()
        {
            // Arrange
            var account = new SavingsAccount { IdentificationNumber = 1, Balance = 50m, AccountStatus = "Active" };
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, true)).ReturnsAsync(new List<SavingsAccount> { account });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => this.savingsService.WithdrawAsync(1, 100m, "Dest", 1));
        }

        #endregion

        #region Account Closure Tests

        [Fact]
        public async Task CloseAccountAsync_WhenClosingFixedDepositWithPenalty_ThenReturnsCloseAccountResponse()
        {
            // Arrange
            var source = new SavingsAccount { IdentificationNumber = 1, Balance = 100m, SavingsType = "FixedDeposit", MaturityDate = DateTime.UtcNow.AddDays(30), AccountStatus = "Active" };
            var dest = new SavingsAccount { IdentificationNumber = 2, AccountStatus = "Active" };
            var response = new ClosureResultDto { Success = true, PenaltyApplied = 2m };

            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, true)).ReturnsAsync(new List<SavingsAccount> { source, dest });
            this.savingsRepositoryMock.Setup(r => r.CloseSavingsAccountAsync(1, 2, 98m, 2m)).ReturnsAsync(response);

            // Act
            var result = await this.savingsService.CloseAccountAsync(1, 2, 1);

            // Assert
            Assert.Equal(response, result);
        }

        #endregion

        #region Transaction & Utility Tests

        [Fact]
        public async Task GetTransactionsAsync_WhenPageNumberIsNegative_ThenThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => this.savingsService.GetTransactionsAsync(1, string.Empty, -1, 10));
        }

        [Fact]
        public async Task GetFundingSourcesAsync_WhenValidRequest_ThenReturnsFundingSources()
        {
            // Arrange
            var sources = new List<FundingSourceOption> { new() { Id = 1, DisplayName = "Bank" } };
            this.savingsRepositoryMock.Setup(r => r.GetFundingSourcesAsync(1)).ReturnsAsync(sources);

            // Act
            var result = await this.savingsService.GetFundingSourcesAsync(1);

            // Assert
            Assert.Equal(sources, result);
        }

        [Fact]
        public void ComputeWithdrawalPenalty_WhenValidAmountProvided_ThenReturnsResult()
        {
            // Act & Assert
            Assert.Equal(2m, this.savingsService.ComputeWithdrawalPenalty(100m));
        }

        [Fact]
        public void GetPenaltyDecimalFor_WhenInvalidPenaltyCase_ThenThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => this.savingsService.GetPenaltyDecimalFor("Invalid"));
        }

        #endregion
    }
}