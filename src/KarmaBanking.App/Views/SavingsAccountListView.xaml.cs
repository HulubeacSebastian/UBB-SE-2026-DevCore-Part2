using KarmaBanking.App.Repositories;
using KarmaBanking.App.Services;
using KarmaBanking.App.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace KarmaBanking.App.Views
{
    public sealed partial class SavingsAccountListView : Page
    {
        private readonly SavingsAccountListViewModel savingsAccountListViewModel;

        public SavingsAccountListView()
        {
            InitializeComponent();
            savingsAccountListViewModel = new SavingsAccountListViewModel(
                new SavingsService(new SavingsRepository()));
            DataContext = savingsAccountListViewModel;
            Loaded += async (sender, args) => await savingsAccountListViewModel.LoadSavingsAccountsAsync(userId: 1);
        }
    }
}
