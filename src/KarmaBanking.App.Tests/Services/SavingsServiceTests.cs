using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using KarmaBanking.App.Repositories.Interfaces;
using NUnit.Framework;
using KarmaBanking.App.Models.DTOs;
using KarmaBanking.App.Services;
using KarmaBanking.App.Models;

namespace KarmaBanking.App.Tests.Services
{
    [TestFixture]
    internal class SavingsServiceTests
    {
        private const decimal FIXED_DEPOSIT_APY = 0.04m;
        private const decimal GOAL_SAVINGS_APY = 0.03m;
        private const decimal HIGH_YIELD_APY = 0.03m;
        private const decimal DEFAULT_APY = 0.02m;

        private const decimal DECIMAL_EARLY_CLOSURE_PENALTY = 0.02m;
        private const decimal DECIMAL_EARLY_WITHDRAWAL_PENALTY = 0.02m;

        [Test]
        public async Task CreateAccountAsync_StandardAccountIsCreated_ReturnsCreatedAccount()
        {
            var repository = Substitute.For<ISavingsRepository>();
            var inputDto = new CreateSavingsAccountDto
            {
                UserId = 1,
                SavingsType = "Standard",
                AccountName = "My Savings",
                InitialDeposit = 1000m,
                FundingAccountId = 123,
            };

            var outputDto = new SavingsAccount
            {
                Id = 1,
                UserId = inputDto.UserId,
                AccountName = inputDto.AccountName,
                Balance = inputDto.InitialDeposit,
                SavingsType = inputDto.SavingsType,
                AccountStatus = "Active"
            };

            var service = new SavingsService(repository);
            repository.CreateSavingsAccountAsync(inputDto, DEFAULT_APY).Returns(Task.FromResult(outputDto));

            await service.CreateAccountAsync(inputDto);

            Assert.Equals(outputDto, await service.CreateAccountAsync(inputDto));
            await repository.Received(1).CreateSavingsAccountAsync(inputDto, DEFAULT_APY);
        }

        [Test]
        public async Task CreateAccountAsync_GoalSavingsAccountIsCreated_ReturnsCreatedAccount()
        {
            var repository = Substitute.For<ISavingsRepository>();
            var inputDto = new CreateSavingsAccountDto
            {
                UserId = 1,
                SavingsType = "GoalSavings",
                AccountName = "My Savings",
                InitialDeposit = 1000m,
                FundingAccountId = 123,
            };

            var outputDto = new SavingsAccount
            {
                Id = 1,
                UserId = inputDto.UserId,
                AccountName = inputDto.AccountName,
                Balance = inputDto.InitialDeposit,
                SavingsType = inputDto.SavingsType,
                AccountStatus = "Active"
            };

            var service = new SavingsService(repository);
            repository.CreateSavingsAccountAsync(inputDto, GOAL_SAVINGS_APY).Returns(Task.FromResult(outputDto));

            await service.CreateAccountAsync(inputDto);

            Assert.Equals(outputDto, await service.CreateAccountAsync(inputDto));
            await repository.Received(1).CreateSavingsAccountAsync(inputDto, GOAL_SAVINGS_APY);
        }

        [Test]
        public async Task CreateAccountAsync_FixedDepositAccountIsCreated_ReturnsCreatedAccount()
        {
            var repository = Substitute.For<ISavingsRepository>();
            var inputDto = new CreateSavingsAccountDto
            {
                UserId = 1,
                SavingsType = "FixedDeposit",
                AccountName = "My Savings",
                InitialDeposit = 1000m,
                FundingAccountId = 123,
            };

            var outputDto = new SavingsAccount
            {
                Id = 1,
                UserId = inputDto.UserId,
                AccountName = inputDto.AccountName,
                Balance = inputDto.InitialDeposit,
                SavingsType = inputDto.SavingsType,
                AccountStatus = "Active"
            };

            var service = new SavingsService(repository);
            repository.CreateSavingsAccountAsync(inputDto, FIXED_DEPOSIT_APY).Returns(Task.FromResult(outputDto));

            await service.CreateAccountAsync(inputDto);

            Assert.Equals(outputDto, await service.CreateAccountAsync(inputDto));
            await repository.Received(1).CreateSavingsAccountAsync(inputDto, FIXED_DEPOSIT_APY);
        }

        [Test]
        public async Task CreateAccountAsync_HighYieldAccountIsCreated_ReturnsCreatedAccount()
        {
            var repository = Substitute.For<ISavingsRepository>();
            var inputDto = new CreateSavingsAccountDto
            {
                UserId = 1,
                SavingsType = "HighYield",
                AccountName = "My Savings",
                InitialDeposit = 1000m,
                FundingAccountId = 123,
            };

            var outputDto = new SavingsAccount
            {
                Id = 1,
                UserId = inputDto.UserId,
                AccountName = inputDto.AccountName,
                Balance = inputDto.InitialDeposit,
                SavingsType = inputDto.SavingsType,
                AccountStatus = "Active"
            };

            var service = new SavingsService(repository);
            repository.CreateSavingsAccountAsync(inputDto, HIGH_YIELD_APY).Returns(Task.FromResult(outputDto));

            await service.CreateAccountAsync(inputDto);

            Assert.Equals(outputDto, await service.CreateAccountAsync(inputDto));
            await repository.Received(1).CreateSavingsAccountAsync(inputDto, HIGH_YIELD_APY);
        }

        [Test]
        public async Task CreateAccountAsync_UserHasMaxActiveAccounts_ThrowsInvalidOperationException()
        {
            var repository = Substitute.For<ISavingsRepository>();
            var userId = 1;

            var activeAccounts = new List<SavingsAccount>
            {
                new SavingsAccount { Id = 1, UserId = userId, AccountStatus = "Active" },
                new SavingsAccount { Id = 2, UserId = userId, AccountStatus = "Active" },
                new SavingsAccount { Id = 3, UserId = userId, AccountStatus = "Active" },
                new SavingsAccount { Id = 4, UserId = userId, AccountStatus = "Active" },
                new SavingsAccount { Id = 5, UserId = userId, AccountStatus = "Active" }
            };
            repository.GetSavingsAccountsByUserIdAsync(userId, false).Returns(Task.FromResult(activeAccounts));

            var service = new SavingsService(repository);
            var dto = new CreateSavingsAccountDto { UserId = userId, SavingsType = "Standard" };

            Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAccountAsync(dto));
            await repository.DidNotReceive().CreateSavingsAccountAsync(Arg.Any<CreateSavingsAccountDto>(), Arg.Any<decimal>());
        }
    }
}
