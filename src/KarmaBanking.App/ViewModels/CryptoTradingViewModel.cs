namespace KarmaBanking.App.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using KarmaBanking.App.Models;
    using KarmaBanking.App.Services.Interfaces;
    using KarmaBanking.App.Utils;

    public class CryptoTradingViewModel : INotifyPropertyChanged
    {
        private readonly IInvestmentService investmentService;

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
            this.SubmitTradeCommand = new RelayCommand(async () => await this.ExecuteTradeAsync(), this.CanExecuteTrade);
            this.LoadWalletBalance();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public RelayCommand SubmitTradeCommand { get; }

        public string SelectedTicker
        {
            get => this.selectedTicker;
            set
            {
                this.selectedTicker = value;
                this.OnPropertyChanged();
                this.UpdateCalculations();
                this.SubmitTradeCommand.RaiseCanExecuteChanged();
            }
        }

        public string ActionType
        {
            get => this.selectedActionType;
            set
            {
                this.selectedActionType = value;
                this.OnPropertyChanged();
                this.UpdateCalculations();
            }
        }

        public string QuantityText
        {
            get => this.quantityText;
            set
            {
                this.quantityText = value;
                this.OnPropertyChanged();
                this.UpdateCalculations();
                this.SubmitTradeCommand.RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => this.statusMessage;
            set
            {
                this.statusMessage = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsSubmitting
        {
            get => this.isSubmitting;
            set
            {
                this.isSubmitting = value;
                this.OnPropertyChanged();
                this.SubmitTradeCommand.RaiseCanExecuteChanged();
            }
        }

        public decimal CurrentBalance
        {
            get => this.currentWalletBalance;
            set
            {
                this.currentWalletBalance = value;
                this.OnPropertyChanged();
            }
        }

        public decimal EstimatedFee
        {
            get => this.estimatedTransactionFee;
            set
            {
                this.estimatedTransactionFee = value;
                this.OnPropertyChanged();
            }
        }

        public decimal TotalAmount
        {
            get => this.totalTransactionAmount;
            set
            {
                this.totalTransactionAmount = value;
                this.OnPropertyChanged();
            }
        }

        private void LoadWalletBalance()
        {
            try
            {
                // Folosim identificatorul hardcodat 1 pentru flow-ul actual al proiectului
                Portfolio userPortfolio = this.investmentService.GetPortfolio(1);
                if (userPortfolio != null)
                {
                    this.CurrentBalance = userPortfolio.TotalValue;
                }
            }
            catch (Exception exception)
            {
                this.StatusMessage = $"Failed to sync wallet balance: {exception.Message}";
            }
        }

        private void UpdateCalculations()
        {
            if (string.IsNullOrWhiteSpace(this.QuantityText) || !decimal.TryParse(this.QuantityText, out decimal quantity) || quantity <= 0)
            {
                this.EstimatedFee = 0;
                this.TotalAmount = 0;
                return;
            }

            // Simulam pretul curent (intr-un scenariu real, acesta vine dintr-un serviciu de Market Data)
            decimal currentMarketPrice = this.SelectedTicker == "BTC" ? 65000m : 3000m;
            decimal tradeValue = quantity * currentMarketPrice;

            // Logica de calcul a comisionului (1.5% cu minim 0.50$)
            decimal calculatedFee = Math.Round(tradeValue * 0.015m, 2);
            this.EstimatedFee = calculatedFee < 0.50m ? 0.50m : calculatedFee;

            if (this.ActionType == "BUY")
            {
                this.TotalAmount = tradeValue + this.EstimatedFee;
            }
            else
            {
                this.TotalAmount = tradeValue - this.EstimatedFee;
            }
        }

        private bool CanExecuteTrade()
        {
            bool hasValidQuantity = decimal.TryParse(this.QuantityText, out decimal quantity) && quantity > 0;

            if (this.IsSubmitting || !hasValidQuantity)
            {
                return false;
            }

            // Validare de baza pentru fonduri insuficiente la cumparare
            if (this.ActionType == "BUY" && this.TotalAmount > this.CurrentBalance)
            {
                return false;
            }

            return true;
        }

        private async Task ExecuteTradeAsync()
        {
            if (!decimal.TryParse(this.QuantityText, out decimal quantity))
            {
                return;
            }

            this.IsSubmitting = true;
            this.StatusMessage = "Executing trade...";

            try
            {
                decimal mockPrice = this.SelectedTicker == "BTC" ? 65000m : 3000m;

                await this.investmentService.ExecuteCryptoTradeAsync(1, this.SelectedTicker, this.ActionType, quantity, mockPrice);

                this.StatusMessage = $"Successfully executed {this.ActionType} for {quantity} {this.SelectedTicker}.";
                this.QuantityText = string.Empty;

                this.LoadWalletBalance();
            }
            catch (Exception exception)
            {
                this.StatusMessage = $"Error: {exception.Message}";
            }
            finally
            {
                this.IsSubmitting = false;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}