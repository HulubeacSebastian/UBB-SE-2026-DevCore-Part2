namespace KarmaBanking.App.Views
{
    using KarmaBanking.App.Models;
    using KarmaBanking.App.Repositories;
    using KarmaBanking.App.ViewModels;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Controls.Primitives;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;

    public sealed partial class InvestmentsView : Page
    {
        // Redenumit pentru a fi semantic conform cerintei
        private const string RefreshPricesEventName = "refreshPrices";
        private readonly ObservableCollection<InvestmentHolding> displayedHoldings;
        private readonly List<ToggleButton> filterButtons;
        private bool hasPageLoaded;
        private string activeFilterType = "All";

        public InvestmentsView()
        {
            this.InitializeComponent();

            // Dependency Injection manual
            this.ViewModel = new InvestmentsViewModel(new InvestmentRepository());
            this.DataContext = this.ViewModel;

            this.displayedHoldings = new ObservableCollection<InvestmentHolding>();
            this.filterButtons = new List<ToggleButton>
            {
                this.AllFilterButton,
                this.StocksFilterButton,
                this.EtfsFilterButton,
                this.BondsFilterButton,
                this.CryptoFilterButton,
                this.OtherFilterButton
            };

            this.HoldingsListView.ItemsSource = this.displayedHoldings;

            this.Loaded += this.OnPageLoaded;
            this.Unloaded += this.OnPageUnloaded;
            this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;
        }

        public InvestmentsViewModel ViewModel { get; }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            if (this.hasPageLoaded)
            {
                return;
            }

            this.hasPageLoaded = true;
            // Redenumit din loadPortfolio() -> LoadUserPortfolio()
            this.ViewModel.LoadUserPortfolio();
            this.RefreshDisplayedHoldings();
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            // Redenumit din stopPolling() -> StopMarketDataPolling()
            this.ViewModel.StopMarketDataPolling();
            this.ViewModel.PropertyChanged -= this.OnViewModelPropertyChanged;
            this.Loaded -= this.OnPageLoaded;
            this.Unloaded -= this.OnPageUnloaded;
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Sincronizat cu noile nume de proprietati din ViewModel
            if (e.PropertyName == nameof(InvestmentsViewModel.UserPortfolio))
            {
                this.RefreshDisplayedHoldings();
            }
            else if (e.PropertyName == RefreshPricesEventName)
            {
                this.RefreshDisplayedHoldings();
            }
            else if (e.PropertyName == nameof(InvestmentsViewModel.IsPortfolioLoading))
            {
                this.UpdateEmptyState();
            }
        }

        private void OnFilterClicked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton selectedButton)
            {
                this.activeFilterType = selectedButton.Tag?.ToString() ?? "All";

                foreach (ToggleButton button in this.filterButtons)
                {
                    button.IsChecked = button == selectedButton;
                }

                this.RefreshDisplayedHoldings();
            }
        }

        private void RefreshDisplayedHoldings()
        {
            this.displayedHoldings.Clear();

            // Sincronizat: portfolio -> UserPortfolio
            IEnumerable<InvestmentHolding> holdings = this.ViewModel.UserPortfolio?.Holdings ?? Enumerable.Empty<InvestmentHolding>();
            foreach (InvestmentHolding holding in holdings.Where(this.MatchesActiveFilter))
            {
                this.displayedHoldings.Add(holding);
            }

            this.UpdateEmptyState();
        }

        private bool MatchesActiveFilter(InvestmentHolding holding)
        {
            string assetType = holding.AssetType?.Trim() ?? string.Empty;

            return this.activeFilterType switch
            {
                "Stocks" => assetType.Equals("Stock", System.StringComparison.OrdinalIgnoreCase)
                    || assetType.Equals("Stocks", System.StringComparison.OrdinalIgnoreCase),
                "ETFs" => assetType.Equals("ETF", System.StringComparison.OrdinalIgnoreCase)
                    || assetType.Equals("ETFs", System.StringComparison.OrdinalIgnoreCase),
                "Bonds" => assetType.Equals("Bond", System.StringComparison.OrdinalIgnoreCase)
                    || assetType.Equals("Bonds", System.StringComparison.OrdinalIgnoreCase),
                "Crypto" => assetType.Equals("Crypto", System.StringComparison.OrdinalIgnoreCase),
                "Other" => !assetType.Equals("Stock", System.StringComparison.OrdinalIgnoreCase)
                    && !assetType.Equals("Stocks", System.StringComparison.OrdinalIgnoreCase)
                    && !assetType.Equals("ETF", System.StringComparison.OrdinalIgnoreCase)
                    && !assetType.Equals("ETFs", System.StringComparison.OrdinalIgnoreCase)
                    && !assetType.Equals("Bond", System.StringComparison.OrdinalIgnoreCase)
                    && !assetType.Equals("Bonds", System.StringComparison.OrdinalIgnoreCase)
                    && !assetType.Equals("Crypto", System.StringComparison.OrdinalIgnoreCase),
                _ => true
            };
        }

        private void UpdateEmptyState()
        {
            // Sincronizat: isLoading -> IsPortfolioLoading
            bool isEmpty = !this.ViewModel.IsPortfolioLoading && this.displayedHoldings.Count == 0;
            this.EmptyStateTextBlock.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
            this.HoldingsListView.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}