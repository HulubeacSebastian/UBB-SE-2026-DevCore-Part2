namespace KarmaBanking.App.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using KarmaBanking.App.Models;
    using KarmaBanking.App.Services;
    using KarmaBanking.App.Services.Interfaces;
    using KarmaBanking.App.Utils;

    public class CryptoTradingViewModel : INotifyPropertyChanged
    {
        private readonly IInvestmentService investmentService;
        private readonly CryptoTradeCalculationService tradeCalculationService;

        private string selectedTicker = "BTC";
        private string selectedActionType = "BUY";
        private string quantityText = string.Empty;
        private string statusMessage = string.Empty;
        private bool isSubmitting;

        private decimal currentWalletBalance;
        private decimal estimatedTransactionFee;
        private decimal totalTransactionAmount;

        public CryptoTradingViewModel(IInvestmentService investmentService)
        {
            this.investmentService = investmentService;
            tradeCalculationService = new CryptoTradeCalculationService();
            SubmitTradeCommand = new RelayCommand(async () => await ExecuteTradeAsync(), CanExecuteTrade);
            LoadWalletBalance();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public RelayCommand SubmitTradeCommand { get; }

        public string SelectedTicker
        {
            get => selectedTicker;
            set
            {
                selectedTicker = value;
                OnPropertyChanged();
                UpdateCalculations();
                SubmitTradeCommand.RaiseCanExecuteChanged();
            }
        }

        public string ActionType
        {
            get => selectedActionType;
            set
            {
                selectedActionType = value;
                OnPropertyChanged();
                UpdateCalculations();
            }
        }

        public string QuantityText
        {
            get => quantityText;
            set
            {
                quantityText = value;
                OnPropertyChanged();
                UpdateCalculations();
                SubmitTradeCommand.RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                statusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsSubmitting
        {
            get => isSubmitting;
            set
            {
                isSubmitting = value;
                OnPropertyChanged();
                SubmitTradeCommand.RaiseCanExecuteChanged();
            }
        }

        public decimal CurrentBalance
        {
            get => currentWalletBalance;
            set
            {
                currentWalletBalance = value;
                OnPropertyChanged();
            }
        }

        public decimal EstimatedFee
        {
            get => estimatedTransactionFee;
            set
            {
                estimatedTransactionFee = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalAmount
        {
            get => totalTransactionAmount;
            set
            {
                totalTransactionAmount = value;
                OnPropertyChanged();
            }
        }

        private void LoadWalletBalance()
        {
            try
            {
                // Folosim identificatorul hardcodat 1 pentru flow-ul actual al proiectului
                Portfolio userPortfolio = investmentService.GetPortfolio(1);
                if (userPortfolio != null)
                {
                    CurrentBalance = userPortfolio.TotalValue;
                }
            }
            catch (Exception exception)
            {
                StatusMessage = $"Failed to sync wallet balance: {exception.Message}";
            }
        }

        private void UpdateCalculations()
        {
            if (!tradeCalculationService.TryParsePositiveQuantity(QuantityText, out decimal quantity))
            {
                EstimatedFee = 0;
                TotalAmount = 0;
                return;
            }

            var (estimatedFee, totalAmount) = tradeCalculationService.CalculateTradePreview(SelectedTicker, ActionType, quantity);
            EstimatedFee = estimatedFee;
            TotalAmount = totalAmount;
        }

        private bool CanExecuteTrade()
        {
            return tradeCalculationService.CanExecuteTrade(IsSubmitting, QuantityText, ActionType, TotalAmount, CurrentBalance);
        }

        private async Task ExecuteTradeAsync()
        {
            if (!tradeCalculationService.TryParsePositiveQuantity(QuantityText, out decimal quantity))
            {
                return;
            }

            IsSubmitting = true;
            StatusMessage = "Executing trade...";

            try
            {
                decimal mockPrice = tradeCalculationService.GetMockMarketPrice(SelectedTicker);

                await investmentService.ExecuteCryptoTradeAsync(1, SelectedTicker, ActionType, quantity, mockPrice);

                StatusMessage = $"Successfully executed {ActionType} for {quantity} {SelectedTicker}.";
                QuantityText = string.Empty;

                LoadWalletBalance();
            }
            catch (Exception exception)
            {
                StatusMessage = $"Error: {exception.Message}";
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}