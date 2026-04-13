namespace KarmaBanking.App.Views
{
    using KarmaBanking.App.Repositories;
    using KarmaBanking.App.Services;
    using KarmaBanking.App.ViewModels;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public sealed partial class CryptoTradingView : Page
    {
        public CryptoTradingView()
        {
            this.InitializeComponent();

            // Dependency Injection manual pentru conformitate cu cerinta de separare a straturilor
            var investmentRepository = new InvestmentRepository();
            var investmentService = new InvestmentService(investmentRepository);

            this.ViewModel = new CryptoTradingViewModel(investmentService);
            this.DataContext = this.ViewModel;
        }

        public CryptoTradingViewModel ViewModel { get; }

        private void OnActionTypeChecked(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel == null)
            {
                return;
            }

            if (sender is RadioButton checkedRadioButton)
            {
                this.ViewModel.ActionType = checkedRadioButton.Content.ToString()?.ToUpper() ?? "BUY";
            }
        }
    }
}