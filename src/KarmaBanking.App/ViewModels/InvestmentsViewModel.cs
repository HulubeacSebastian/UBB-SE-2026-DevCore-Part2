namespace KarmaBanking.App.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using KarmaBanking.App.Models;
    using KarmaBanking.App.Repositories.Interfaces;
    using KarmaBanking.App.Services;
    using Microsoft.UI.Dispatching;

    public class InvestmentsViewModel : INotifyPropertyChanged
    {
        private const string RefreshPricesEventName = "refreshPrices";
        private readonly IInvestmentRepository investmentRepository;
        private readonly MarketDataService marketDataService;
        private readonly DispatcherQueue? dispatcherQueue;

        private Portfolio userPortfolio;
        private bool isPortfolioLoading;

        public InvestmentsViewModel(IInvestmentRepository investmentRepository)
        {
            this.investmentRepository = investmentRepository;
            this.marketDataService = new MarketDataService();
            this.dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            this.marketDataService.onPriceUpdate(this.RefreshHoldingPrices);
            this.userPortfolio = new Portfolio();
        }

        public Portfolio UserPortfolio
        {
            get => this.userPortfolio;
            set
            {
                this.userPortfolio = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsPortfolioLoading
        {
            get => this.isPortfolioLoading;
            set
            {
                this.isPortfolioLoading = value;
                this.OnPropertyChanged();
            }
        }

        public void LoadUserPortfolio()
        {
            this.IsPortfolioLoading = true;

            try
            {
                // Hardcoded identification number for initial integration
                this.UserPortfolio = this.investmentRepository.GetPortfolio(1);
                this.marketDataService.startPolling(this.UserPortfolio.Holdings.Select(holding => holding.Ticker).ToList());
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"LoadUserPortfolio error: {exception.Message}");
            }
            finally
            {
                this.IsPortfolioLoading = false;
            }
        }

        public void RefreshHoldingPrices()
        {
            if (this.dispatcherQueue != null && !this.dispatcherQueue.HasThreadAccess)
            {
                this.dispatcherQueue.TryEnqueue(this.RefreshHoldingPrices);
                return;
            }

            if (this.UserPortfolio?.Holdings == null || this.UserPortfolio.Holdings.Count == 0)
            {
                return;
            }

            foreach (var holding in this.UserPortfolio.Holdings)
            {
                decimal updatedPrice = this.marketDataService.getPrice(holding.Ticker);
                if (updatedPrice <= 0)
                {
                    continue;
                }

                holding.CurrentPrice = updatedPrice;
                holding.UnrealizedGainLoss = (holding.CurrentPrice - holding.AveragePurchasePrice) * holding.Quantity;
            }

            this.UserPortfolio.TotalValue = this.UserPortfolio.Holdings.Sum(holding => holding.CurrentPrice * holding.Quantity);
            this.UserPortfolio.TotalGainLoss = this.UserPortfolio.Holdings.Sum(holding => holding.UnrealizedGainLoss);

            decimal totalCost = this.UserPortfolio.Holdings.Sum(holding => holding.AveragePurchasePrice * holding.Quantity);
            this.UserPortfolio.GainLossPercent = totalCost > 0 ? (this.UserPortfolio.TotalGainLoss / totalCost) * 100 : 0;

            this.OnPropertyChanged(nameof(this.UserPortfolio));
            this.OnPropertyChanged(RefreshPricesEventName);
        }

        public void StopMarketDataPolling()
        {
            this.marketDataService.stopPolling();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}