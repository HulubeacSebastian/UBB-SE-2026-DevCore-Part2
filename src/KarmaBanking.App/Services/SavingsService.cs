using KarmaBanking.App.Models;
using KarmaBanking.App.Repositories.Interfaces;
using KarmaBanking.App.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KarmaBanking.App.Services
{
    public class SavingsService : ISavingsService
    {
        private readonly ISavingsRepository savingsRepository;

        public SavingsService(ISavingsRepository savingsRepository)
        {
            this.savingsRepository = savingsRepository;
        }

        public async Task<List<SavingsAccount>> GetSavingsAccountsByUserIdAsync(int userId)
        {
            return await savingsRepository.GetSavingsAccountsByUserIdAsync(userId);
        }

        public async Task<bool> CreateSavingsAccountAsync(SavingsAccount savingsAccount)
        {
            if (savingsAccount.Balance <= 0)
                return false;

            savingsAccount.CreatedAt = DateTime.Now;
            savingsAccount.AccountStatus = "Active";
            savingsAccount.AccruedInterest = 0;

            return await savingsRepository.AddSavingsAccountAsync(savingsAccount);
        }

        public async Task<bool> DepositAsync(int savingsAccountId, decimal depositAmount)
        {
            if (depositAmount <= 0)
                return false;

            var accounts = await savingsRepository.GetSavingsAccountsByUserIdAsync(1);
            var account = accounts.FirstOrDefault(a => a.Id == savingsAccountId);

            if (account == null || account.AccountStatus == "Closed")
                return false;

            return await savingsRepository.UpdateSavingsAccountBalanceAsync(savingsAccountId, depositAmount);
        }

        public async Task<bool> CloseSavingsAccountAsync(int accountId)
        {
            return await savingsRepository.CloseSavingsAccountAsync(accountId);
        }

        public async Task<bool> UpdateSavingsAccountBalanceAsync(int accountId, decimal amount)
        {
            return await savingsRepository.UpdateSavingsAccountBalanceAsync(accountId, amount);
        }

        public async Task ProcessSchedulesAsync()
        {
            var schedules = await savingsRepository.GetAllSchedulesAsync();
            var accounts = await savingsRepository.GetSavingsAccountsByUserIdAsync(1);

            foreach (var schedule in schedules)
            {
                var account = accounts.FirstOrDefault(a => a.Id == schedule.AccountId);

                if (account == null || account.AccountStatus == "Closed")
                    continue;

                await savingsRepository.UpdateSavingsAccountBalanceAsync(
                    schedule.AccountId,
                    schedule.Amount);
            }
        }

        public async Task<bool> CreateScheduleAsync(int savingsAccountId, decimal amount, string frequency)
        {
            var accounts = await savingsRepository.GetSavingsAccountsByUserIdAsync(1);
            var account = accounts.FirstOrDefault(a => a.Id == savingsAccountId);

            if (account == null || account.AccountStatus == "Closed")
                return false;

            return await savingsRepository.CreateScheduleAsync(savingsAccountId, amount, frequency);
        }

    }
}
