using KarmaBanking.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KarmaBanking.App.Services
{
    public class SavingsPresentationService
    {
        public string BuildTotalSavedAmount(IEnumerable<SavingsAccount> accounts)
            => $"${accounts.Sum(account => account.Balance):F2}";

        public string BuildNumberOfAccountsText(int accountCount)
            => $"across {accountCount} account{(accountCount == 1 ? string.Empty : "s")}";

        public string BuildBestInterestRate(IEnumerable<SavingsAccount> accounts)
        {
            decimal bestApy = accounts.Any() ? accounts.Max(account => account.Apy) : 0m;
            return $"{bestApy * 100:F2}%";
        }

        public bool HasClosePenaltyRisk(SavingsAccount? selectedAccount)
        {
            return selectedAccount?.SavingsType == "FixedDeposit" &&
                   selectedAccount.MaturityDate.HasValue &&
                   selectedAccount.MaturityDate.Value > DateTime.UtcNow;
        }
    }
}
