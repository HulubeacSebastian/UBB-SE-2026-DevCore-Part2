using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KarmaBanking.App.Models;
using KarmaBanking.App.Models.DTOs;
using KarmaBanking.App.Repositories.Interfaces;
using KarmaBanking.App.Services.Interfaces;

namespace KarmaBanking.App.Services
{
    public class SavingsService : ISavingsService
    {
        private readonly ISavingsRepository savingsRepository;
        private const int MaxActiveAccounts = 5;

        public SavingsService(ISavingsRepository savingsRepository)
        {
            this.savingsRepository = savingsRepository;
        }

        public async Task<SavingsAccount> CreateAccountAsync(CreateSavingsAccountDto dto)
        {
            // BA-9: max 5 active accounts per user
            var existing = await savingsRepository.GetByUserIdAsync(dto.UserId, includesClosed: false);
            if (existing.Count >= MaxActiveAccounts)
                throw new InvalidOperationException("You cannot have more than 5 active savings accounts.");

            // BA-9: GoalSavings requires a future target date
            if (dto.SavingsType == "GoalSavings")
            {
                if (!dto.TargetDate.HasValue)
                    throw new ArgumentException("GoalSavings accounts require a target date.");

                if (dto.TargetDate.Value <= DateTime.Today)
                    throw new ArgumentException("Target date must be in the future.");

                if (!dto.TargetAmount.HasValue || dto.TargetAmount.Value <= 0)
                    throw new ArgumentException("GoalSavings accounts require a positive target amount.");
            }

            return await savingsRepository.CreateAsync(dto);
        }

        public Task<List<SavingsAccount>> GetAccountsAsync(int userId, bool includesClosed = false)
        {
            return savingsRepository.GetByUserIdAsync(userId, includesClosed);
        }

        public async Task<DepositResponseDto> DepositAsync(int accountId, decimal amount, string source, int userId)
        {
            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be positive.");

            // BA-14: get the account and validate ownership + status
            var accounts = await savingsRepository.GetByUserIdAsync(userId, includesClosed: true);
            var account = accounts.Find(a => a.Id == accountId)
                ?? throw new InvalidOperationException("Account not found or does not belong to you.");

            // BA-14: 422 for closed or locked
            if (account.AccountStatus == "Closed")
                throw new InvalidOperationException("Cannot deposit into a closed account.");

            if (account.AccountStatus == "Locked")
                throw new InvalidOperationException("Cannot deposit into a locked account.");

            return await savingsRepository.DepositAsync(accountId, amount, source);
        }

        public Task<bool> CloseAccountAsync(int accountId)
        {
            return savingsRepository.CloseAsync(accountId);
        }

        public Task<List<FundingSourceOption>> GetFundingSourcesAsync(int userId)
        {
            return savingsRepository.GetFundingSourcesAsync(userId);
        }
    }
}
