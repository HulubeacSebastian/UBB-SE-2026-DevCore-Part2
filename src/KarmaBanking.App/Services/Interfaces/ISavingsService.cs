using System.Collections.Generic;
using System.Threading.Tasks;
using KarmaBanking.App.Models;
using KarmaBanking.App.Models.DTOs;

namespace KarmaBanking.App.Services.Interfaces
{
    public interface ISavingsService
    {
        Task<SavingsAccount> CreateAccountAsync(CreateSavingsAccountDto dto);
        Task<List<SavingsAccount>> GetAccountsAsync(int userId, bool includesClosed = false);
        Task<DepositResponseDto> DepositAsync(int accountId, decimal amount, string source, int userId);
        Task<bool> CloseAccountAsync(int accountId);
        Task<List<FundingSourceOption>> GetFundingSourcesAsync(int userId);
    }
}
