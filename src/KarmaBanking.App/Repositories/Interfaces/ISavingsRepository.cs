using System.Collections.Generic;
using System.Threading.Tasks;
using KarmaBanking.App.Models;
using KarmaBanking.App.Models.DTOs;

namespace KarmaBanking.App.Repositories.Interfaces
{
    public interface ISavingsRepository
    {
        Task<SavingsAccount> CreateAsync(CreateSavingsAccountDto dto);
        Task<List<SavingsAccount>> GetByUserIdAsync(int userId, bool includesClosed = false);
        Task<DepositResponseDto> DepositAsync(int accountId, decimal amount, string source);
        Task<bool> CloseAsync(int accountId);
        Task<List<FundingSourceOption>> GetFundingSourcesAsync(int userId);
    }
}
