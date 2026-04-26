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
    using KarmaBanking.App.Repositories.Interfaces;
    using KarmaBanking.App.Services;
    using Moq;
    using Xunit;

    public class SavingsServiceTests
    {
        private const decimal DefaultAnnualPercentageYield = 0.02m;
        private readonly Mock<ISavingsRepository> savingsRepositoryMock;
        private readonly SavingsService savingsService;

        public SavingsServiceTests()
        {
            this.savingsRepositoryMock = new Mock<ISavingsRepository>();
            this.savingsService = new SavingsService(this.savingsRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateAccountAsync_StandardAccountIsCreated_ReturnsCreatedAccount()
        {
            // Arrange
            var createAccountDataTransferObject = new CreateSavingsAccountDto
            {
                UserIdentificationNumber = 1,
                SavingsType = "Standard",
                AccountName = "My Savings",
                InitialDeposit = 1000m
            };

            var expectedSavingsAccount = new SavingsAccount
            {
                IdentificationNumber = 1,
                UserIdentificationNumber = 1,
                AccountName = "My Savings",
                Balance = 1000m,
                AccountStatus = "Active"
            };

            this.savingsRepositoryMock.Setup(repository => repository.CreateSavingsAccountAsync(createAccountDataTransferObject, DefaultAnnualPercentageYield))
                .ReturnsAsync(expectedSavingsAccount);
            this.savingsRepositoryMock.Setup(repository => repository.GetSavingsAccountsByUserIdAsync(1, false))
                .ReturnsAsync(new List<SavingsAccount>());

            // Act
            var actualSavingsAccount = await this.savingsService.CreateAccountAsync(createAccountDataTransferObject);

            // Assert
            Assert.Equal(expectedSavingsAccount, actualSavingsAccount);
            this.savingsRepositoryMock.Verify(repository => repository.CreateSavingsAccountAsync(createAccountDataTransferObject, DefaultAnnualPercentageYield), Times.Once);
        }

        [Fact]
        public async Task CreateAccountAsync_UserHasMaxActiveAccounts_ThrowsInvalidOperationException()
        {
            // Arrange
            int userIdentificationNumber = 1;
            var activeAccountsList = new List<SavingsAccount>();
            for (int i = 0; i < 5; i++)
            {
                activeAccountsList.Add(new SavingsAccount { AccountStatus = "Active" });
            }

            this.savingsRepositoryMock.Setup(repository => repository.GetSavingsAccountsByUserIdAsync(userIdentificationNumber, false))
                .ReturnsAsync(activeAccountsList);

            var createAccountDataTransferObject = new CreateSavingsAccountDto { UserIdentificationNumber = userIdentificationNumber, SavingsType = "Standard" };

            // Act & Assert
            var validationException = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await this.savingsService.CreateAccountAsync(createAccountDataTransferObject));

            Assert.Equal("You cannot have more than 5 active savings accounts.", validationException.Message);
            this.savingsRepositoryMock.Verify(repository => repository.CreateSavingsAccountAsync(It.IsAny<CreateSavingsAccountDto>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public async Task DepositAsync_ValidDeposit_ReturnsDepositResponse()
        {
            // Arrange
            int userIdentificationNumber = 1;
            int accountIdentificationNumber = 1;
            decimal depositAmountValue = 100m;
            string fundingSourceDescription = "Source";

            var existingAccountInstance = new SavingsAccount
            {
                IdentificationNumber = accountIdentificationNumber,
                UserIdentificationNumber = userIdentificationNumber,
                AccountStatus = "Active"
            };
            var expectedDepositResponse = new DepositResponseDto { NewBalance = 100m };

            this.savingsRepositoryMock.Setup(repository => repository.GetSavingsAccountsByUserIdAsync(userIdentificationNumber, true))
                .ReturnsAsync(new List<SavingsAccount> { existingAccountInstance });
            this.savingsRepositoryMock.Setup(repository => repository.DepositAsync(accountIdentificationNumber, depositAmountValue, fundingSourceDescription))
                .ReturnsAsync(expectedDepositResponse);

            // Act
            var actualDepositResponse = await this.savingsService.DepositAsync(accountIdentificationNumber, depositAmountValue, fundingSourceDescription, userIdentificationNumber);

            savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true)
                .Returns(Task.FromResult(new List<SavingsAccount> { sourceAccount, destinationAccount }));
            savingsRepository.CloseSavingsAccountAsync(accountId, destinationAccountId, transferedAmount, 2).Returns(Task.FromResult(expectedResponse));

            var result = await savingsService.CloseAccountAsync(accountId, destinationAccountId, userId);
            Assert.Equal(expectedResponse, result);
            await savingsRepository.Received(1).GetSavingsAccountsByUserIdAsync(userId, true);
            await savingsRepository.Received(1).CloseSavingsAccountAsync(accountId, destinationAccountId, transferedAmount, 2);
        }

        [Fact]
        public async Task CloseAccountAsync_CloseFixedDepositWithoutMaturityDate_ReturnsCloseAccountResponse()
        {
            var userId = 1;
            var accountId = 1;
            var destinationAccountId = 2;
            var transferedAmount = 100m;

            var sourceAccount = new SavingsAccount
            {
                IdentificationNumber = accountId,
                UserIdentificationNumber = userId,
                AccountStatus = AccountStatus.Active.ToString(),
                Balance = 100m,
                SavingsType = "FixedDeposit",
            };

            var destinationAccount = new SavingsAccount
            {
                IdentificationNumber = destinationAccountId,
                UserIdentificationNumber = userId,
                AccountStatus = AccountStatus.Active.ToString(),
                Balance = 100m,
            };

            var expectedResponse = new ClosureResultDto
            {
                Success = true,
                TransferredAmount = transferedAmount,
                Message = "Account closed without penalty.",
                ClosedAt = DateTime.UtcNow,
                PenaltyApplied = 0,
            };

            savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true)
                .Returns(Task.FromResult(new List<SavingsAccount> { sourceAccount, destinationAccount }));
            savingsRepository.CloseSavingsAccountAsync(accountId, destinationAccountId, transferedAmount, 0).Returns(Task.FromResult(expectedResponse));

            var result = await savingsService.CloseAccountAsync(accountId, destinationAccountId, userId);
            Assert.Equal(expectedResponse, result);
            await savingsRepository.Received(1).GetSavingsAccountsByUserIdAsync(userId, true);
            await savingsRepository.Received(1).CloseSavingsAccountAsync(accountId, destinationAccountId, transferedAmount, 0);
        }

        [Fact]
        public async Task CloseAccountAsync_CloseMaturedFixedDepositWithoutPenalty_ReturnsCloseAccountResponse()
        {
            var userId = 1;
            var accountId = 1;
            var destinationAccountId = 2;
            var transferedAmount = 100m;

            var sourceAccount = new SavingsAccount
            {
                IdentificationNumber = accountId,
                UserIdentificationNumber = userId,
                AccountStatus = AccountStatus.Active.ToString(),
                Balance = 100m,
                SavingsType = "FixedDeposit",
                MaturityDate = DateTime.UtcNow.AddDays(-1),
            };

            var destinationAccount = new SavingsAccount
            {
                IdentificationNumber = destinationAccountId,
                UserIdentificationNumber = userId,
                AccountStatus = AccountStatus.Active.ToString(),
                Balance = 100m,
            };

            var expectedResponse = new ClosureResultDto
            {
                Success = true,
                TransferredAmount = transferedAmount,
                Message = "Account closed without penalty.",
                ClosedAt = DateTime.UtcNow,
                PenaltyApplied = 0,
            };

            savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true)
                .Returns(Task.FromResult(new List<SavingsAccount> { sourceAccount, destinationAccount }));
            savingsRepository.CloseSavingsAccountAsync(accountId, destinationAccountId, transferedAmount, 0).Returns(Task.FromResult(expectedResponse));

            var result = await savingsService.CloseAccountAsync(accountId, destinationAccountId, userId);
            Assert.Equal(expectedResponse, result);
            await savingsRepository.Received(1).GetSavingsAccountsByUserIdAsync(userId, true);
            await savingsRepository.Received(1).CloseSavingsAccountAsync(accountId, destinationAccountId, transferedAmount, 0);
        }

        [Fact]
        public async Task CloseAccountAsync_CloseStandardAccountWithoutPenalty_ReturnsCloseAccountResponse()
        {
            var userId = 1;
            var accountId = 1;
            var destinationAccountId = 2;
            var transferedAmount = 100m;

            var sourceAccount = new SavingsAccount
            {
                IdentificationNumber = accountId,
                UserIdentificationNumber = userId,
                AccountStatus = AccountStatus.Active.ToString(),
                Balance = 100m,
                SavingsType = "Standard",
            };

            var destinationAccount = new SavingsAccount
            {
                IdentificationNumber = destinationAccountId,
                UserIdentificationNumber = userId,
                AccountStatus = AccountStatus.Active.ToString(),
                Balance = 100m,
            };

            var expectedResponse = new ClosureResultDto
            {
                Success = true,
                TransferredAmount = transferedAmount,
                Message = "Account closed without penalty.",
                ClosedAt = DateTime.UtcNow,
                PenaltyApplied = 0,
            };

            savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true)
                .Returns(Task.FromResult(new List<SavingsAccount> { sourceAccount, destinationAccount }));
            savingsRepository.CloseSavingsAccountAsync(accountId, destinationAccountId, transferedAmount, 0).Returns(Task.FromResult(expectedResponse));

            var result = await savingsService.CloseAccountAsync(accountId, destinationAccountId, userId);
            Assert.Equal(expectedResponse, result);
            await savingsRepository.Received(1).GetSavingsAccountsByUserIdAsync(userId, true);
            await savingsRepository.Received(1).CloseSavingsAccountAsync(accountId, destinationAccountId, transferedAmount, 0);
        }

        [Fact]
        public async Task CloseAccountAsync_CloseGoalSavingsAccountWithoutPenalty_ReturnsCloseAccountResponse()
        {
            var userId = 1;
            var accountId = 1;
            var destinationAccountId = 2;
            var transferedAmount = 100m;

            var sourceAccount = new SavingsAccount
            {
                IdentificationNumber = accountId,
                UserIdentificationNumber = userId,
                AccountStatus = AccountStatus.Active.ToString(),
                Balance = 100m,
                SavingsType = "GoalSavings",
            };

            var destinationAccount = new SavingsAccount
            {
                IdentificationNumber = destinationAccountId,
                UserIdentificationNumber = userId,
                AccountStatus = AccountStatus.Active.ToString(),
                Balance = 100m,
            };

            var expectedResponse = new ClosureResultDto
            {
                Success = true,
                TransferredAmount = transferedAmount,
                Message = "Account closed without penalty.",
                ClosedAt = DateTime.UtcNow,
                PenaltyApplied = 0,
            };

            savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true)
                .Returns(Task.FromResult(new List<SavingsAccount> { sourceAccount, destinationAccount }));
            savingsRepository.CloseSavingsAccountAsync(accountId, destinationAccountId, transferedAmount, 0).Returns(Task.FromResult(expectedResponse));

            var result = await savingsService.CloseAccountAsync(accountId, destinationAccountId, userId);
            Assert.Equal(expectedResponse, result);
            await savingsRepository.Received(1).GetSavingsAccountsByUserIdAsync(userId, true);
            await savingsRepository.Received(1).CloseSavingsAccountAsync(accountId, destinationAccountId, transferedAmount, 0);
        }

        [Fact]
        public async Task CloseAccountAsync_CloseHighYieldAccountWithoutPenalty_ReturnsCloseAccountResponse()
        {
            var userId = 1;
            var accountId = 1;
            var destinationAccountId = 2;
            var transferedAmount = 100m;

            var sourceAccount = new SavingsAccount
            {
                IdentificationNumber = accountId,
                UserIdentificationNumber = userId,
                AccountStatus = AccountStatus.Active.ToString(),
                Balance = 100m,
                SavingsType = "HighYield",
            };

            var destinationAccount = new SavingsAccount
            {
                IdentificationNumber = destinationAccountId,
                UserIdentificationNumber = userId,
                AccountStatus = AccountStatus.Active.ToString(),
                Balance = 100m,
            };

            var expectedResponse = new ClosureResultDto
            {
                Success = true,
                TransferredAmount = transferedAmount,
                Message = "Account closed without penalty.",
                ClosedAt = DateTime.UtcNow,
                PenaltyApplied = 0,
            };

            savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true)
                .Returns(Task.FromResult(new List<SavingsAccount> { sourceAccount, destinationAccount }));
            savingsRepository.CloseSavingsAccountAsync(accountId, destinationAccountId, transferedAmount, 0).Returns(Task.FromResult(expectedResponse));

            var result = await savingsService.CloseAccountAsync(accountId, destinationAccountId, userId);
            Assert.Equal(expectedResponse, result);
            await savingsRepository.Received(1).GetSavingsAccountsByUserIdAsync(userId, true);
            await savingsRepository.Received(1).CloseSavingsAccountAsync(accountId, destinationAccountId, transferedAmount, 0);
        }

        [Fact]
        public async Task WithdrawAsync_NegativeAmount_ThrowsArgumentException()
        {
            var accountId = 1;
            var userId = 1;

            var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await savingsService.WithdrawAsync(accountId, -100m, "Destination", userId));
            Assert.Equal("Withdrawal amount must be positive.", ex.Message);
            await savingsRepository.DidNotReceive().GetSavingsAccountsByUserIdAsync(userId, true);
            await savingsRepository.DidNotReceive().WithdrawAsync(Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<decimal>());
        }

        [Fact]
        public async Task WithdrawAsync_DestinationAccountNotFound_ThrowsInvalidOperationException()
        {
            var accountId = 1;
            var userId = 1;
            var inexistentDestinationAccountId = 999;

            savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true)
                .Returns(Task.FromResult(new List<SavingsAccount>
                {
                    new SavingsAccount
                    {
                        IdentificationNumber = accountId,
                        UserIdentificationNumber = userId,
                        AccountStatus = AccountStatus.Active.ToString()
                    }
                }));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await savingsService.WithdrawAsync(inexistentDestinationAccountId, 100m, "Destination label", userId));
            Assert.Equal("Account not found or does not belong to you.", ex.Message);
            await savingsRepository.Received(1).GetSavingsAccountsByUserIdAsync(userId, true);
            await savingsRepository.DidNotReceive().WithdrawAsync(Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<decimal>());
        }

        [Fact]
        public async Task WithdrawAsync_DestinationAccountClosed_ThrowsInvalidOperationException()
        {
            var accountId = 1;
            var userId = 1;

            savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true)
                .Returns(Task.FromResult(new List<SavingsAccount>
                {
                    new SavingsAccount
                    {
                        IdentificationNumber = accountId,
                        UserIdentificationNumber = userId,
                        AccountStatus = AccountStatus.Closed.ToString(),
                    }
                }));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await savingsService.WithdrawAsync(accountId, 100m, "Destination label", userId));
            Assert.Equal("Cannot withdraw from a closed account.", ex.Message);
            await savingsRepository.Received(1).GetSavingsAccountsByUserIdAsync(userId, true);
            await savingsRepository.DidNotReceive().WithdrawAsync(Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<decimal>());
        }

        [Fact]
        public async Task WithdrawAsync_InsufficientBalance_ThrowsInvalidOperationException()
        {
            var accountId = 1;
            var userId = 1;

            savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true)
                .Returns(Task.FromResult(new List<SavingsAccount>
                {
                    new SavingsAccount
                    {
                        IdentificationNumber = accountId,
                        UserIdentificationNumber = userId,
                        AccountStatus = AccountStatus.Active.ToString(),
                        Balance = 50m,
                    }
                }));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await savingsService.WithdrawAsync(accountId, 100m, "Destination label", userId));
            Assert.Equal("Insufficient balance.", ex.Message);
            await savingsRepository.Received(1).GetSavingsAccountsByUserIdAsync(userId, true);
            await savingsRepository.DidNotReceive().WithdrawAsync(Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<decimal>());
        }

        [Fact]
        public async Task WithdrawAsync_FixedDepositPenaltyAndInsufficientBalanceAfterPenalty_ThrowsInvalidOperationException()
        {
            var accountId = 1;
            var userId = 1;

            savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true)
                .Returns(Task.FromResult(new List<SavingsAccount>
                {
                    new SavingsAccount
                    {
                        IdentificationNumber = accountId,
                        UserIdentificationNumber = userId,
                        AccountStatus = AccountStatus.Active.ToString(),
                        Balance = 100m,
                        SavingsType = "FixedDeposit",
                        MaturityDate = DateTime.UtcNow.AddDays(30),
                    }
                }));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await savingsService.WithdrawAsync(accountId, 100m, "Destination label", userId));
            Assert.Equal("Insufficient balance after penalty.", ex.Message);
            await savingsRepository.Received(1).GetSavingsAccountsByUserIdAsync(userId, true);
            await savingsRepository.DidNotReceive().WithdrawAsync(Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<decimal>());
        }

        [Fact]
        public async Task WithdrawAsync_FixedDepositWithoutMaturityDate_ReturnsWithdrawResponseDto()
        {
            var accountId = 1;
            var userId = 1;

            var expectedResponse = new WithdrawResponseDto
            {
                Success = true,
                AmountWithdrawn = 50m,
                ProcessedAt = DateTime.UtcNow,
                NewBalance = 0m,
                Message = "Withdrawal successful without penalty.",
                PenaltyApplied = 0m,
            };

            savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true)
                .Returns(Task.FromResult(new List<SavingsAccount>
                {
                    new SavingsAccount
                    {
                        IdentificationNumber = accountId,
                        UserIdentificationNumber = userId,
                        AccountStatus = AccountStatus.Active.ToString(),
                        Balance = 100m,
                        SavingsType = "FixedDeposit",
                    }
                }));
            savingsRepository.WithdrawAsync(accountId, 50m, "Destination label", 0m).Returns(Task.FromResult(expectedResponse));

            var response = await savingsService.WithdrawAsync(accountId, 50m, "Destination label", userId);
            Assert.Equal(expectedResponse, response);
            await savingsRepository.Received(1).GetSavingsAccountsByUserIdAsync(userId, true);
            await savingsRepository.Received(1).WithdrawAsync(accountId, 50m, "Destination label", 0m);
        }

        [Fact]
        public async Task WithdrawAsync_MaturedFixedDeposit_ReturnsWithdrawResponseDto()
        {
            var accountId = 1;
            var userId = 1;

            var expectedResponse = new WithdrawResponseDto
            {
                Success = true,
                AmountWithdrawn = 50m,
                ProcessedAt = DateTime.UtcNow,
                NewBalance = 0m,
                Message = "Withdrawal successful without penalty.",
                PenaltyApplied = 0m,
            };

            savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true)
                .Returns(Task.FromResult(new List<SavingsAccount>
                {
                    new SavingsAccount
                    {
                        IdentificationNumber = accountId,
                        UserIdentificationNumber = userId,
                        AccountStatus = AccountStatus.Matured.ToString(),
                        Balance = 100m,
                        SavingsType = "FixedDeposit",
                        MaturityDate = DateTime.UtcNow.AddDays(-1),
                    }
                }));
            savingsRepository.WithdrawAsync(accountId, 50m, "Destination label", 0m).Returns(Task.FromResult(expectedResponse));

            var response = await savingsService.WithdrawAsync(accountId, 50m, "Destination label", userId);
            Assert.Equal(expectedResponse, response);
            await savingsRepository.Received(1).GetSavingsAccountsByUserIdAsync(userId, true);
            await savingsRepository.Received(1).WithdrawAsync(accountId, 50m, "Destination label", 0m);
        }

        [Fact]
        public async Task WithdrawAsync_StandardSavingsAccount_ReturnsWithdrawResponseDto()
        {
            var accountId = 1;
            var userId = 1;

            var expectedResponse = new WithdrawResponseDto
            {
                Success = true,
                AmountWithdrawn = 50m,
                ProcessedAt = DateTime.UtcNow,
                NewBalance = 0m,
                Message = "Withdrawal successful without penalty.",
                PenaltyApplied = 0m,
            };

            savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true)
                .Returns(Task.FromResult(new List<SavingsAccount>
                {
                    new SavingsAccount
                    {
                        IdentificationNumber = accountId,
                        UserIdentificationNumber = userId,
                        AccountStatus = AccountStatus.Matured.ToString(),
                        Balance = 100m,
                        SavingsType = "Standard",
                    }
                }));
            savingsRepository.WithdrawAsync(accountId, 50m, "Destination label", 0m).Returns(Task.FromResult(expectedResponse));

            var response = await savingsService.WithdrawAsync(accountId, 50m, "Destination label", userId);
            Assert.Equal(expectedResponse, response);
            await savingsRepository.Received(1).GetSavingsAccountsByUserIdAsync(userId, true);
            await savingsRepository.Received(1).WithdrawAsync(accountId, 50m, "Destination label", 0m);
        }

        [Fact]
        public async Task WithdrawAsync_GoalSavingsAccount_ReturnsWithdrawResponseDto()
        {
            var accountId = 1;
            var userId = 1;

            var expectedResponse = new WithdrawResponseDto
            {
                Success = true,
                AmountWithdrawn = 50m,
                ProcessedAt = DateTime.UtcNow,
                NewBalance = 0m,
                Message = "Withdrawal successful without penalty.",
                PenaltyApplied = 0m,
            };

            savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true)
                .Returns(Task.FromResult(new List<SavingsAccount>
                {
                    new SavingsAccount
                    {
                        IdentificationNumber = accountId,
                        UserIdentificationNumber = userId,
                        AccountStatus = AccountStatus.Matured.ToString(),
                        Balance = 100m,
                        SavingsType = "GoalSavings",
                    }
                }));
            savingsRepository.WithdrawAsync(accountId, 50m, "Destination label", 0m).Returns(Task.FromResult(expectedResponse));

            var response = await savingsService.WithdrawAsync(accountId, 50m, "Destination label", userId);
            Assert.Equal(expectedResponse, response);
            await savingsRepository.Received(1).GetSavingsAccountsByUserIdAsync(userId, true);
            await savingsRepository.Received(1).WithdrawAsync(accountId, 50m, "Destination label", 0m);
        }

        [Fact]
        public async Task WithdrawAsync_HighYieldSavingsAccount_ReturnsWithdrawResponseDto()
        {
            var accountId = 1;
            var userId = 1;

            var expectedResponse = new WithdrawResponseDto
            {
                Success = true,
                AmountWithdrawn = 50m,
                ProcessedAt = DateTime.UtcNow,
                NewBalance = 0m,
                Message = "Withdrawal successful without penalty.",
                PenaltyApplied = 0m,
            };

            savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true)
                .Returns(Task.FromResult(new List<SavingsAccount>
                {
                    new SavingsAccount
                    {
                        IdentificationNumber = accountId,
                        UserIdentificationNumber = userId,
                        AccountStatus = AccountStatus.Matured.ToString(),
                        Balance = 100m,
                        SavingsType = "HighYield",
                    }
                }));
            savingsRepository.WithdrawAsync(accountId, 50m, "Destination label", 0m).Returns(Task.FromResult(expectedResponse));

            var response = await savingsService.WithdrawAsync(accountId, 50m, "Destination label", userId);
            Assert.Equal(expectedResponse, response);
            await savingsRepository.Received(1).GetSavingsAccountsByUserIdAsync(userId, true);
            await savingsRepository.Received(1).WithdrawAsync(accountId, 50m, "Destination label", 0m);
        }

        [Fact]
        public async Task GetAutoDepositAsync_ValidCase_ReturnsAutoDeposit()
        {
            var accountId = 1;

            var expectedAutoDeposit = new AutoDeposit
            {
                Id = 1,
                Amount = 100m,
                Frequency = DepositFrequency.Monthly,
                NextRunDate = DateTime.UtcNow.AddDays(30),
                SavingsAccountId = accountId,
                IsActive = true,
            };

            savingsRepository.GetAutoDepositAsync(accountId).Returns(Task.FromResult<AutoDeposit?>(expectedAutoDeposit));

            var result = await savingsService.GetAutoDepositAsync(accountId);
            Assert.Equal(expectedAutoDeposit, result);
            await savingsRepository.Received(1).GetAutoDepositAsync(accountId);
        }

        [Fact]
        public async Task SaveAutoDepositAsync_ValidCase_ReturnsAutoDeposit()
        {
            var accountId = 1;

            var autoDeposit = new AutoDeposit
            {
                Id = 1,
                Amount = 100m,
                Frequency = DepositFrequency.Monthly,
                NextRunDate = DateTime.UtcNow.AddDays(30),
                SavingsAccountId = accountId,
                IsActive = true,
            };

            savingsRepository.SaveAutoDepositAsync(autoDeposit).Returns(Task.CompletedTask);

            await savingsService.SaveAutoDepositAsync(autoDeposit);
            await savingsRepository.Received(1).SaveAutoDepositAsync(autoDeposit);
        }

        [Fact]
        public async Task GetFundingSourcesAsync_ValidCase_ReturnsFundingSources()
        {
            var userId = 1;
            var expectedFundingSources = new List<FundingSourceOption>
            {
                new FundingSourceOption
                {
                    Id = 1,
                    DisplayName = "My Bank Account"
                },
                new FundingSourceOption
                {
                    Id = 2,
                    DisplayName = "My Credit Card"
                },
            };

            savingsRepository.GetFundingSourcesAsync(userId).Returns(Task.FromResult(expectedFundingSources));

            var result = await savingsService.GetFundingSourcesAsync(userId);
            Assert.Equal(expectedFundingSources, result);
            await savingsRepository.Received(1).GetFundingSourcesAsync(userId);
        }

        [Fact]
        public async Task GetTransactionsAsync_NegativePage_ThrowsArgumentException()
        {
            var accountId = 1;

            var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await savingsService.GetTransactionsAsync(accountId, "filter", -1, 10));
            Assert.Equal("Page must be >= 1", ex.Message);
            await savingsRepository.DidNotReceive().GetTransactionsPagedAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>());
        }

        [Fact]
        public async Task GetTransactionsAsync_NegativePageSize_ReturnsResult()
        {
            var accountId = 1;
            var transactionsList = new List<SavingsTransaction>
            {
                new SavingsTransaction
                {
                    Id = 1,
                    SavingsAccountId = accountId,
                    Amount = 100m,
                    Type = TransactionType.Deposit,
                    Source = "Test",
                    CreatedAt = DateTime.UtcNow,
                    AccountId = accountId,
                    BalanceAfter = 100m,
                    Description = "Test transaction",
                }
            };

            savingsRepository.GetTransactionsPagedAsync(accountId, "filter", 1, 20)
                .Returns(Task.FromResult((transactionsList, 1)));

            var result = await savingsService.GetTransactionsAsync(accountId, "filter", 1, -10);
            Assert.Equal((transactionsList, 1), result);
            await savingsRepository.Received(1).GetTransactionsPagedAsync(accountId, "filter", 1, 20);
        }

        [Fact]
        public async Task GetTransactionsAsync_TooBigPageSize_ReturnsResult()
        {
            var accountId = 1;
            var transactionsList = new List<SavingsTransaction>
            {
                new SavingsTransaction
                {
                    Id = 1,
                    SavingsAccountId = accountId,
                    Amount = 100m,
                    Type = TransactionType.Deposit,
                    Source = "Test",
                    CreatedAt = DateTime.UtcNow,
                    AccountId = accountId,
                    BalanceAfter = 100m,
                    Description = "Test transaction",
                }
            };

            savingsRepository.GetTransactionsPagedAsync(accountId, "filter", 1, 20)
                .Returns(Task.FromResult((transactionsList, 1)));

            var result = await savingsService.GetTransactionsAsync(accountId, "filter", 1, 1000);
            Assert.Equal((transactionsList, 1), result);
            await savingsRepository.Received(1).GetTransactionsPagedAsync(accountId, "filter", 1, 20);
        }

        [Fact]
        public async Task GetTransactionsAsync_ValidParameters_ReturnsResult()
        {
            var accountId = 1;
            var transactionsList = new List<SavingsTransaction>
            {
                new SavingsTransaction
                {
                    Id = 1,
                    SavingsAccountId = accountId,
                    Amount = 100m,
                    Type = TransactionType.Deposit,
                    Source = "Test",
                    CreatedAt = DateTime.UtcNow,
                    AccountId = accountId,
                    BalanceAfter = 100m,
                    Description = "Test transaction",
                }
            };

            savingsRepository.GetTransactionsPagedAsync(accountId, "filter", 1, 10)
                .Returns(Task.FromResult((transactionsList, 1)));

            var result = await savingsService.GetTransactionsAsync(accountId, "filter", 1, 10);
            Assert.Equal((transactionsList, 1), result);
            await savingsRepository.Received(1).GetTransactionsPagedAsync(accountId, "filter", 1, 10);
        }

        [Fact]
        public async Task GetValidTransferDestinationsAsync_ValidCase_ReturnsDestinations()
        {
            var accountId = 1;
            var userId = 1;
            var account1 = new SavingsAccount
            {
                IdentificationNumber = 1,
                AccountName = "My Savings Account 1",
            };
            var account2 = new SavingsAccount
            {
                IdentificationNumber = 2,
                AccountName = "My Savings Account 2",
            };

            var destinations = new List<SavingsAccount>
            {
                account1, account2
            };

            var expectedDestinations = new List<SavingsAccount>
            {
                account2
            };

            savingsRepository.GetSavingsAccountsByUserIdAsync(userId)
                .Returns(Task.FromResult(destinations));

            var result = await savingsService.GetValidTransferDestinationsAsync(accountId);
            Assert.Equal(expectedDestinations, result);
            await savingsRepository.Received(1).GetSavingsAccountsByUserIdAsync(userId);
        }

        [Fact]
        public async Task ComputeWithdrawalPenalty_ValidInput_ReturnsResult()
        {
            var amount = 100m;
            var expectedResult = 2m;

            var result = savingsService.ComputeWithdrawalPenalty(amount);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task HasRiskEarlyWithdrawal_FixedDepositNotMatured_ReturnsTrue()
        {
            var account = new SavingsAccount
            {
                SavingsType = "FixedDeposit",
                MaturityDate = DateTime.UtcNow.AddDays(30),
            };

            var result = savingsService.HasRiskEarlyWithdrawal(account);
            Assert.True(result);
        }

        [Fact]
        public async Task HasRiskEarlyWithdrawal_FixedDepositMatured_ReturnsFalse()
        {
            var account = new SavingsAccount
            {
                SavingsType = "FixedDeposit",
                MaturityDate = DateTime.UtcNow.AddDays(-1),
            };

            var result = savingsService.HasRiskEarlyWithdrawal(account);
            Assert.False(result);
        }

        [Fact]
        public async Task HasRiskEarlyWithdrawal_FixedDepositWithoutMaturityDate_ReturnsFalse()
        {
            var account = new SavingsAccount
            {
                SavingsType = "FixedDeposit",
            };

            var result = savingsService.HasRiskEarlyWithdrawal(account);
            Assert.False(result);
        }

        [Fact]
        public async Task HasRiskEarlyWithdrawal_StandardSavingsAccount_ReturnsFalse()
        {
            var account = new SavingsAccount
            {
                SavingsType = "Standard",
            };
            var result = savingsService.HasRiskEarlyWithdrawal(account);
            Assert.False(result);
        }

        [Fact]
        public async Task HasRiskEarlyWithdrawal_GoalSavingsAccount_ReturnsFalse()
        {
            var account = new SavingsAccount
            {
                SavingsType = "GoalSavings",
            };
            var result = savingsService.HasRiskEarlyWithdrawal(account);
            Assert.False(result);
        }

        [Fact]
        public async Task HasRiskEarlyWithdrawal_HighYield_ReturnsFalse()
        {
            var account = new SavingsAccount
            {
                SavingsType = "HighYield",
            };
            var result = savingsService.HasRiskEarlyWithdrawal(account);
            Assert.False(result);
        }

        [Fact]
        public async Task GetPenaltyDecimalFor_EarlyWithdrawal_ReturnsExpectedValue()
        {
            var expectedPenalty = DECIMAL_EARLY_WITHDRAWAL_PENALTY;
            var result = savingsService.GetPenaltyDecimalFor("EarlyWithdrawal");
            Assert.Equal(expectedPenalty, result);
        }

        [Fact]
        public async Task GetPenaltyDecimalFor_EarlyClosure_ReturnsExpectedValue()
        {
            var expectedPenalty = DECIMAL_EARLY_CLOSURE_PENALTY;
            var result = savingsService.GetPenaltyDecimalFor("EarlyClosure");
            Assert.Equal(expectedPenalty, result);
        }

        [Fact]
        public async Task GetPenaltyDecimalFor_InvalidPenaltyCase_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => savingsService.GetPenaltyDecimalFor("Invalid penalty case"));
            Assert.Equal("Invalid penalty case.", ex.Message);
        }
    }
}