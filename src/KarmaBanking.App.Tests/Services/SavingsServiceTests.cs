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
        public async Task CreateAccountAsync_StandardAccountIsCreated_ReturnsCreatedAccount()
        {
            var inputDto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "Standard", InitialDeposit = 1000m };
            var outputAccount = new SavingsAccount { IdentificationNumber = 1, UserIdentificationNumber = 1, Balance = 1000m };

            this.savingsRepositoryMock.Setup(r => r.CreateSavingsAccountAsync(inputDto, DefaultAnnualPercentageYield)).ReturnsAsync(outputAccount);
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(new List<SavingsAccount>());

            var result = await this.savingsService.CreateAccountAsync(inputDto);

            Assert.Equal(outputAccount, result);
        }

        [Fact]
        public async Task CreateAccountAsync_GoalSavingsAccountIsCreated_ReturnsCreatedAccount()
        {
            var inputDto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "GoalSavings", TargetAmount = 5000m, TargetDate = DateTime.UtcNow.AddDays(30) };
            var outputAccount = new SavingsAccount { IdentificationNumber = 1, UserIdentificationNumber = 1, TargetAmount = 5000m };

            this.savingsRepositoryMock.Setup(r => r.CreateSavingsAccountAsync(inputDto, GoalSavingsAnnualPercentageYield)).ReturnsAsync(outputAccount);
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(new List<SavingsAccount>());

            var result = await this.savingsService.CreateAccountAsync(inputDto);

            Assert.Equal(outputAccount, result);
        }

        [Fact]
        public async Task CreateAccountAsync_FixedDepositAccountIsCreated_ReturnsCreatedAccount()
        {
            var inputDto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "FixedDeposit" };
            this.savingsRepositoryMock.Setup(r => r.CreateSavingsAccountAsync(inputDto, FixedDepositAnnualPercentageYield)).ReturnsAsync(new SavingsAccount());
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(new List<SavingsAccount>());

            await this.savingsService.CreateAccountAsync(inputDto);

            this.savingsRepositoryMock.Verify(r => r.CreateSavingsAccountAsync(inputDto, FixedDepositAnnualPercentageYield), Times.Once);
        }

        [Fact]
        public async Task CreateAccountAsync_HighYieldAccountIsCreated_ReturnsCreatedAccount()
        {
            var inputDto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "HighYield" };
            this.savingsRepositoryMock.Setup(r => r.CreateSavingsAccountAsync(inputDto, HighYieldAnnualPercentageYield)).ReturnsAsync(new SavingsAccount());
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(new List<SavingsAccount>());

            await this.savingsService.CreateAccountAsync(inputDto);

            this.savingsRepositoryMock.Verify(r => r.CreateSavingsAccountAsync(inputDto, HighYieldAnnualPercentageYield), Times.Once);
        }

        [Fact]
        public async Task CreateAccountAsync_UserHasMaxActiveAccounts_ThrowsInvalidOperationException()
        {
            var activeAccounts = new List<SavingsAccount> { new(), new(), new(), new(), new() };
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(activeAccounts);

            var dto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "Standard" };
            await Assert.ThrowsAsync<InvalidOperationException>(() => this.savingsService.CreateAccountAsync(dto));
        }

        [Fact]
        public async Task CreateAccountAsync_GoalSavingsWithoutTargetDate_ThrowsArgumentException()
        {
            var dto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "GoalSavings", TargetAmount = 5000m };
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(new List<SavingsAccount>());

            await Assert.ThrowsAsync<ArgumentException>(() => this.savingsService.CreateAccountAsync(dto));
        }

        [Fact]
        public async Task CreateAccountAsync_GoalSavingsWithPastTargetDate_ThrowsArgumentException()
        {
            var dto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "GoalSavings", TargetDate = DateTime.UtcNow.AddDays(-1) };
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(new List<SavingsAccount>());

            await Assert.ThrowsAsync<ArgumentException>(() => this.savingsService.CreateAccountAsync(dto));
        }

        [Fact]
        public async Task CreateAccountAsync_GoalSavingsWithoutTargetAmount_ThrowsArgumentException()
        {
            var dto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "GoalSavings", TargetDate = DateTime.UtcNow.AddDays(30) };
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(new List<SavingsAccount>());

            await Assert.ThrowsAsync<ArgumentException>(() => this.savingsService.CreateAccountAsync(dto));
        }

        [Fact]
        public async Task CreateAccountAsync_GoalSavingsWithNegativeTargetAmount_ThrowsArgumentException()
        {
            var dto = new CreateSavingsAccountDto { UserIdentificationNumber = 1, SavingsType = "GoalSavings", TargetDate = DateTime.UtcNow.AddDays(30), TargetAmount = -100m };
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, false)).ReturnsAsync(new List<SavingsAccount>());

            await Assert.ThrowsAsync<ArgumentException>(() => this.savingsService.CreateAccountAsync(dto));
        }

        #endregion

        #region Deposit & Withdrawal Tests

        [Fact]
        public async Task DepositAsync_NegativeAmount_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => this.savingsService.DepositAsync(1, -10m, "Source", 1));
        }

        [Fact]
        public async Task DepositAsync_InvalidAccountId_ThrowsInvalidOperationException()
        {
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, true)).ReturnsAsync(new List<SavingsAccount>());
            await Assert.ThrowsAsync<InvalidOperationException>(() => this.savingsService.DepositAsync(99, 100m, "Source", 1));
        }

        [Fact]
        public async Task DepositAsync_AccountStatusClosed_ThrowsInvalidOperationException()
        {
            var account = new SavingsAccount { IdentificationNumber = 1, AccountStatus = AccountStatus.Closed.ToString() };
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, true)).ReturnsAsync(new List<SavingsAccount> { account });

            await Assert.ThrowsAsync<InvalidOperationException>(() => this.savingsService.DepositAsync(1, 100m, "Source", 1));
        }

        [Fact]
        public async Task DepositAsync_ValidDeposit_ReturnsDepositResponse()
        {
            var account = new SavingsAccount { IdentificationNumber = 1, UserIdentificationNumber = 1, AccountStatus = "Active" };
            var response = new DepositResponseDto { NewBalance = 500m };
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, true)).ReturnsAsync(new List<SavingsAccount> { account });
            this.savingsRepositoryMock.Setup(r => r.DepositAsync(1, 100m, "Source")).ReturnsAsync(response);

            var result = await this.savingsService.DepositAsync(1, 100m, "Source", 1);

            Assert.Equal(response, result);
        }

        [Fact]
        public async Task WithdrawAsync_InsufficientBalance_ThrowsInvalidOperationException()
        {
            var account = new SavingsAccount { IdentificationNumber = 1, Balance = 50m, AccountStatus = "Active" };
            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, true)).ReturnsAsync(new List<SavingsAccount> { account });

            await Assert.ThrowsAsync<InvalidOperationException>(() => this.savingsService.WithdrawAsync(1, 100m, "Dest", 1));
        }

        #endregion

        #region Account Closure Tests

        [Fact]
        public async Task CloseAccountAsync_CloseFixedDepositWithPenalty_ReturnsCloseAccountResponse()
        {
            var source = new SavingsAccount { IdentificationNumber = 1, Balance = 100m, SavingsType = "FixedDeposit", MaturityDate = DateTime.UtcNow.AddDays(30), AccountStatus = "Active" };
            var dest = new SavingsAccount { IdentificationNumber = 2, AccountStatus = "Active" };
            var response = new ClosureResultDto { Success = true, PenaltyApplied = 2m };

            this.savingsRepositoryMock.Setup(r => r.GetSavingsAccountsByUserIdAsync(1, true)).ReturnsAsync(new List<SavingsAccount> { source, dest });
            this.savingsRepositoryMock.Setup(r => r.CloseSavingsAccountAsync(1, 2, 98m, 2m)).ReturnsAsync(response);

            var result = await this.savingsService.CloseAccountAsync(1, 2, 1);

            Assert.Equal(response, result);
        }

        #endregion

        #region Transaction & Utility Tests

        [Fact]
        public async Task GetTransactionsAsync_NegativePage_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => this.savingsService.GetTransactionsAsync(1, string.Empty, -1, 10));
        }

        [Fact]
        public async Task GetFundingSourcesAsync_ValidCase_ReturnsFundingSources()
        {
            var sources = new List<FundingSourceOption> { new() { Id = 1, DisplayName = "Bank" } };
            this.savingsRepositoryMock.Setup(r => r.GetFundingSourcesAsync(1)).ReturnsAsync(sources);

            var result = await this.savingsService.GetFundingSourcesAsync(1);

            Assert.Equal(sources, result);
        }

        [Fact]
        public void ComputeWithdrawalPenalty_ValidInput_ReturnsResult()
        {
            Assert.Equal(2m, this.savingsService.ComputeWithdrawalPenalty(100m));
        }

        [Fact]
        public void GetPenaltyDecimalFor_InvalidPenaltyCase_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => this.savingsService.GetPenaltyDecimalFor("Invalid"));
        }

        #endregion
    }
}