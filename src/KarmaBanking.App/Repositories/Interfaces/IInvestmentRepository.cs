using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarmaBanking.App.Repositories.Interfaces
{
    internal interface IInvestmentRepository
    {
        Task RecordCryptoTradeAsync(int portfolioId, string ticker, string actionType, decimal quantity, decimal pricePerUnit, decimal fees);
    }
}
