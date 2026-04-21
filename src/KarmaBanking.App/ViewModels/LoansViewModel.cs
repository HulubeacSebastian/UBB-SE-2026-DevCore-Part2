using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KarmaBanking.App.Services;
using KarmaBanking.App.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KarmaBanking.App.ViewModels;

public partial class LoansViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly AmortizationCalculator _calculator;
    private readonly PdfExporter _pdfExporter;
    private readonly LoanDialogStateService _loanDialogStateService;
    private readonly LoanApplicationPresentationService _loanApplicationPresentationService;
    public IEnumerable<LoanType> LoanTypes => Enum.GetValues<LoanType>();
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    // --- Aici e fix-ul pentru lista care nu apărea la deschidere ---
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredLoans))]
    private ObservableCollection<LoanViewModel> loans = [];

    [ObservableProperty] private ObservableCollection<AmortizationRow> amortizationRows = [];
    [ObservableProperty] private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string errorMessage = string.Empty;

    [ObservableProperty] private LoanType selectedLoanType;
    [ObservableProperty] private double desiredAmount;
    [ObservableProperty] private int preferredTermMonths;
    [ObservableProperty] private string purpose = string.Empty;
    [ObservableProperty] private LoanEstimate currentEstimate;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ApplicationWasApproved))]
    private string applicationResult = string.Empty;
    [ObservableProperty]
    private bool applicationWasApproved;
    [ObservableProperty] private LoanViewModel selectedLoan;
    [ObservableProperty] private double? customAmount;

    [NotifyPropertyChangedFor(nameof(FilteredLoans))]
    [ObservableProperty] private LoanStatus? statusFilter = null;

    [NotifyPropertyChangedFor(nameof(FilteredLoans))]
    [ObservableProperty] private LoanType? typeFilter = null;

    // --- Proprietăți noi pentru controlul UI-ului din Dialog ---
    [ObservableProperty] private string dialogTitle = "Apply for Loan";
    [ObservableProperty] private string dialogActionText = "Continue";
    [ObservableProperty] private bool isFormVisible = true;
    [ObservableProperty] private bool isReviewVisible = false;
    [ObservableProperty] private bool isEstimateVisible = false;

    // --- Payment preview properties ---
    [ObservableProperty] private decimal paymentPreviewBalance = 0m;
    [ObservableProperty] private int paymentPreviewRemainingMonths = 0;
    
    private readonly PaymentCalculationService _paymentCalculationService = new();

    public IEnumerable<LoanViewModel> FilteredLoans =>
        loans.Where(l =>
       (statusFilter == null || l.Loan.LoanStatus == statusFilter) && (typeFilter == null || l.Loan.LoanType == typeFilter));

    public LoansViewModel()
    {
        _apiService = new ApiService(new LoanService(new LoanRepository()));
        _calculator = new AmortizationCalculator();
        _pdfExporter = new PdfExporter();
        _loanDialogStateService = new LoanDialogStateService();
        _loanApplicationPresentationService = new LoanApplicationPresentationService();
        _ = LoadLoansAsync();
    }

    [RelayCommand]
    public async Task LoadLoansAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _apiService.GetLoansByUserAsync(CurrentUser.Id);
            Loans = new ObservableCollection<LoanViewModel>(
                result.Select(l => new LoanViewModel(l)));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ApplyForLoanAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var request = new LoanApplicationRequest
            {
                UserId = CurrentUser.Id,
                LoanType = SelectedLoanType,
                DesiredAmount = (decimal)DesiredAmount,
                PreferredTermMonths = PreferredTermMonths,
                Purpose = Purpose
            };

            var rejectionReason = await _apiService.ApplyForLoanAsync(request);

            var applicationOutcome = _loanApplicationPresentationService.BuildApplicationOutcome(rejectionReason);
            ApplicationResult = applicationOutcome.Message;
            ApplicationWasApproved = applicationOutcome.Approved;
            if (applicationOutcome.Approved)
            {
                await LoadLoansAsync();
                OnPropertyChanged(nameof(FilteredLoans));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void ComputeLiveEstimate()
    {
        ErrorMessage = string.Empty;
        try
        {
            var request = new LoanApplicationRequest
            {
                UserId = CurrentUser.Id,
                LoanType = SelectedLoanType,
                DesiredAmount = (decimal)DesiredAmount,
                PreferredTermMonths = PreferredTermMonths,
                Purpose = Purpose
            };
            CurrentEstimate = _apiService.GetLoanEstimate(request);
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }

    public async Task PayInstallmentAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            decimal? amount = CustomAmount.HasValue
                ? (decimal?)CustomAmount.Value
                : null;
            await _apiService.PayInstallmentAsync(SelectedLoan.Loan.Id, amount);
            await LoadLoansAsync();

            OnPropertyChanged(nameof(FilteredLoans));
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Updates the payment preview (balance and remaining term) based on selected payment method.
    /// </summary>
    public void UpdatePaymentPreview(bool isStandardPayment, string customAmountText = "")
    {
        if (SelectedLoan == null)
        {
            PaymentPreviewBalance = 0m;
            PaymentPreviewRemainingMonths = 0;
            return;
        }

        Loan loan = SelectedLoan.Loan;
        decimal customAmount = 0m;

        if (!isStandardPayment && !string.IsNullOrWhiteSpace(customAmountText))
        {
            var (success, amount) = _paymentCalculationService.ParsePaymentAmount(customAmountText);
            if (!success)
            {
                customAmount = 0m;
            }
            else
            {
                customAmount = amount;
            }
        }

        var (balance, months) = _paymentCalculationService.CalculatePaymentPreview(
            loan.MonthlyInstallment,
            loan.OutstandingBalance,
            loan.RemainingMonths,
            isStandardPayment,
            customAmount
        );

        PaymentPreviewBalance = balance;
        PaymentPreviewRemainingMonths = months;
    }

    public void SelectStandardPayment()
    {
        CustomAmount = null;
        UpdatePaymentPreview(isStandardPayment: true);
    }

    public string SelectCustomPayment()
    {
        if (SelectedLoan == null)
        {
            CustomAmount = null;
            UpdatePaymentPreview(isStandardPayment: false, string.Empty);
            return string.Empty;
        }

        if (!CustomAmount.HasValue)
        {
            decimal initialCustomAmount = _paymentCalculationService.GetInitialCustomAmount(
                SelectedLoan.Loan.MonthlyInstallment,
                SelectedLoan.Loan.OutstandingBalance,
                CustomAmount);
            CustomAmount = (double)initialCustomAmount;
        }
        else
        {
            decimal normalizedCustomAmount = _paymentCalculationService.GetInitialCustomAmount(
                SelectedLoan.Loan.MonthlyInstallment,
                SelectedLoan.Loan.OutstandingBalance,
                CustomAmount);
            CustomAmount = (double)normalizedCustomAmount;
        }

        string currentText = _paymentCalculationService.FormatCustomAmount((decimal)(CustomAmount ?? 0d));
        UpdatePaymentPreview(isStandardPayment: false, currentText);
        return currentText;
    }

    public void UpdateCustomPayment(string customAmountText)
    {
        var (success, amount) = _paymentCalculationService.ParsePaymentAmount(customAmountText);
        CustomAmount = success ? (double)amount : null;
        UpdatePaymentPreview(isStandardPayment: false, customAmountText);
    }

    public async Task LoadAmortizationAsync()
    {
        if (SelectedLoan == null)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var rows = await _apiService.GetAmortizationAsync(SelectedLoan.Loan.Id);
            AmortizationRows = new ObservableCollection<AmortizationRow>(rows);
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DownloadSchedulePdfAsync()
    {
        try
        {
            var rows = await _apiService.GetAmortizationAsync(SelectedLoan.Loan.Id);
            byte[] pdfBytes = _pdfExporter.exportAmortization(rows);
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string fileName = $"amortization_schedule_{SelectedLoan.Loan.Id}.pdf";
            string filePath = Path.Combine(desktopPath, fileName);

            await File.WriteAllBytesAsync(filePath, pdfBytes);

            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }

    // --- Metode de control pentru Dialog ---
    public void SwitchToReviewStage()
    {
        IsFormVisible = false;
        IsReviewVisible = true;
        DialogTitle = "Application Review";
        DialogActionText = "Submit";
    }

    public void ResetDialogState()
    {
        IsFormVisible = true;
        IsReviewVisible = false;
        DialogTitle = "Apply for Loan";
        DialogActionText = "Continue";
        ApplicationResult = string.Empty;
        ApplicationWasApproved = false;

        DesiredAmount = 0;
        PreferredTermMonths = 0;
        Purpose = string.Empty;
        CurrentEstimate = null;
        IsEstimateVisible = false;
    }

    partial void OnDesiredAmountChanged(double value) => TryComputeEstimate();
    partial void OnPreferredTermMonthsChanged(int value) => TryComputeEstimate();
    partial void OnSelectedLoanTypeChanged(LoanType value) => TryComputeEstimate();
    partial void OnPurposeChanged(string value) => TryComputeEstimate();

    private void TryComputeEstimate()
    {
        bool isFullyFilled = _loanDialogStateService.ShouldComputeEstimate(
            DesiredAmount,
            PreferredTermMonths,
            Purpose);

        if (isFullyFilled)
        {
            ComputeLiveEstimate();
            IsEstimateVisible = true;
        }
        else
        {
            CurrentEstimate = null;
            IsEstimateVisible = false;
        }
    }
}
