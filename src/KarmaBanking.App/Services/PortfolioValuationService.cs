using KarmaBanking.App.Models;
using System.Collections.Generic;
using System.Linq;

namespace KarmaBanking.App.Services
{
    public class PortfolioValuationService
    {
        public void UpdateHoldingValuation(InvestmentHolding holding, decimal updatedPrice)
        {
            holding.CurrentPrice = updatedPrice;
            holding.UnrealizedGainLoss = (holding.CurrentPrice - holding.AveragePurchasePrice) * holding.Quantity;
        }

        public void UpdatePortfolioTotals(Portfolio portfolio)
        {
            List<InvestmentHolding> holdings = portfolio.Holdings;
            portfolio.TotalValue = holdings.Sum(holding => holding.CurrentPrice * holding.Quantity);
            portfolio.TotalGainLoss = holdings.Sum(holding => holding.UnrealizedGainLoss);

            decimal totalCost = holdings.Sum(holding => holding.AveragePurchasePrice * holding.Quantity);
            portfolio.GainLossPercent = totalCost > 0 ? (portfolio.TotalGainLoss / totalCost) * 100 : 0;
        }
    }
}
