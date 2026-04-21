using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KarmaBanking.App.Models;
using KarmaBanking.App.Models.DTOs;
using KarmaBanking.App.Models.Enums;
using KarmaBanking.App.Services;
using KarmaBanking.App.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KarmaBanking.App.ViewModels
{
    public partial class SavingsViewModel : BaseViewModel
    {
        private readonly ISavingsService savingsService;
        private readonly SavingsUiRulesService savingsUiRulesService;
        private readonly SavingsPresentationService savingsPresentationService;
        private readonly SavingsWorkflowService savingsWorkflowService;
        private const int CurrentUserId = 1;

        // ── My Accounts ──────────────────────────────────────────────────────

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEmpty))]
        [NotifyPropertyChangedFor(nameof(ShowAccountsList))]
        private ObservableCollection<SavingsAccount> savingsAccounts = new();

        [ObservableProperty] private string totalSavedAmount = "$0.00";
        [ObservableProperty] private string numberOfAccountsText = "across 0 accounts";
        [ObservableProperty] private string bestInterestRate = "0.00%";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LivePreview))]
        [NotifyPropertyChangedFor(nameof(WithdrawHasEarlyRisk))]
        [NotifyPropertyChangedFor(nameof(WithdrawPenaltySummary))]
        [NotifyPropertyChangedFor(nameof(WithdrawEstimatedPenalty))]
        [NotifyPropertyChangedFor(nameof(WithdrawNetAmount))]
        [NotifyPropertyChangedFor(nameof(WithdrawHasPenalty))]
        [NotifyPropertyChangedFor(nameof(WithdrawPenaltyBreakdownText))]
        [NotifyPropertyChangedFor(nameof(WithdrawNetAmountText))]
        [NotifyPropertyChangedFor(nameof(CloseHasPenalty))]
        private SavingsAccount? selectedAccount;

        public bool IsEmpty => SavingsAccounts.Count == 0;
        public bool ShowAccountsList => SavingsAccounts.Count > 0;

        // ── Create Account ───────────────────────────────────────────────────

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsGoalSavings))]
        [NotifyPropertyChangedFor(nameof(IsFixedDeposit))]
        private string selectedSavingsType = string.Empty;

        [ObservableProperty] private string accountName = string.Empty;
        [ObservableProperty] private string initialDepositText = string.Empty;
        [ObservableProperty] private FundingSourceOption? selectedFundingSource;
        [ObservableProperty] private decimal? targetAmount;
        [ObservableProperty] private DateTimeOffset? targetDate;
        [ObservableProperty] private bool showCreateConfirmation;
        [ObservableProperty] private ObservableCollection<FundingSourceOption> fundingSources = new();
        [ObservableProperty] private string selectedFrequency = string.Empty;

        public bool IsGoalSavings => SelectedSavingsType == "GoalSavings";
        public bool IsFixedDeposit => SelectedSavingsType == "FixedDeposit";
        public Dictionary<string, string> FieldErrors { get; } = new();

        // ── Deposit ──────────────────────────────────────────────────────────

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LivePreview))]
        private string depositAmountText = string.Empty;

        [ObservableProperty] private string depositSource = string.Empty;
        [ObservableProperty] private bool showDepositSuccess;
        [ObservableProperty] private string depositSuccessMessage = string.Empty;

        private CancellationTokenSource? depositCancelationTokenSource;

        public string LivePreview
        {
            get
            {
                return savingsUiRulesService.BuildDepositPreview(DepositAmountText, SelectedAccount);
            }
        }

        // ── Withdraw Panel ───────────────────────────────────────────────────
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WithdrawEstimatedPenalty))]
        [NotifyPropertyChangedFor(nameof(WithdrawNetAmount))]
        [NotifyPropertyChangedFor(nameof(WithdrawHasPenalty))]
        [NotifyPropertyChangedFor(nameof(WithdrawPenaltyBreakdownText))]
        [NotifyPropertyChangedFor(nameof(WithdrawNetAmountText))]
        private string withdrawAmountText = string.Empty;

        [ObservableProperty] private FundingSourceOption? withdrawDestination;
        [ObservableProperty] private string withdrawResultMessage = string.Empty;
        [ObservableProperty] private bool withdrawSuccess;

        public bool WithdrawHasEarlyRisk => savingsService.HasRiskEarlyWithdrawal(SelectedAccount);

        public decimal WithdrawEstimatedPenalty
        {
            get
            {
                if (!WithdrawHasEarlyRisk) return 0;
                if (!savingsUiRulesService.TryParsePositiveAmount(WithdrawAmountText, out decimal withdrawAmount)) return 0;
                return savingsService.ComputeWithdrawalPenalty(withdrawAmount);
            }
        }

        public decimal WithdrawNetAmount
        {
            get
            {
                if (!savingsUiRulesService.TryParsePositiveAmount(WithdrawAmountText, out decimal withdrawAmount)) return 0;
                return savingsUiRulesService.CalculateWithdrawNetAmount(withdrawAmount, WithdrawEstimatedPenalty);
            }
        }

        public bool WithdrawHasPenalty => WithdrawEstimatedPenalty > 0;

        public string WithdrawPenaltyBreakdownText =>
            $"Penalty ({savingsService.GetPenaltyDecimalFor("EarlyWithdrawal"):P0}): -${WithdrawEstimatedPenalty:N2}";

        public string WithdrawNetAmountText => $"Net amount received: ${WithdrawNetAmount:N2}";

        public string WithdrawPenaltySummary =>
            WithdrawHasEarlyRisk ? $"Early withdrawal penalty: {savingsService.GetPenaltyDecimalFor("EarlyWithdrawal"):P2} of amount. Maturity date: {SelectedAccount?.MaturityDate:d}" : string.Empty;

        public async Task<bool> ConfirmWithdrawAsync()
        {
            WithdrawResultMessage = string.Empty;
            WithdrawSuccess = false;
            savingsUiRulesService.TryParsePositiveAmount(WithdrawAmountText, out decimal amount);
            var withdrawValidation = savingsWorkflowService.ValidateWithdrawRequest(amount, WithdrawDestination);
            if (!withdrawValidation.IsValid) { WithdrawResultMessage = withdrawValidation.ErrorMessage; return false; }
            IsLoading = true;
            try
            {
                var withdrawResponseDto = await savingsService.WithdrawAsync(SelectedAccount!.Id, amount, WithdrawDestination.DisplayName, CurrentUserId);
                WithdrawSuccess = withdrawResponseDto.Success;
                WithdrawResultMessage = savingsWorkflowService.BuildWithdrawResultMessage(withdrawResponseDto);
                if (withdrawResponseDto.Success)
                {
                    WithdrawAmountText = string.Empty;
                    await LoadAccountsAsync();
                }
                return withdrawResponseDto.Success;
            }
            catch (Exception exception) { WithdrawResultMessage = exception.Message; return false; }
            finally { IsLoading = false; }
        }

        // ── Auto Deposit ─────────────────────────────────────────────────────

        private AutoDeposit? currentAutoDeposit;

        [ObservableProperty] private string autoDepositAmountText = string.Empty;
        [ObservableProperty] private string autoDepositFrequency = string.Empty;
        [ObservableProperty] private DateTimeOffset? autoDepositStartDate = DateTimeOffset.Now.AddDays(1);
        [ObservableProperty] private bool autoDepositIsActive = true;
        [ObservableProperty] private bool hasExistingAutoDeposit;
        public string ExistingLabel => HasExistingAutoDeposit ? "Modify" : "Set Up";
        [ObservableProperty] private string autoDepositSaveMessage = string.Empty;

        public async Task LoadAutoDepositAsync(int accountId)
        {
            AutoDepositSaveMessage = string.Empty;
            currentAutoDeposit = await savingsService.GetAutoDepositAsync(accountId);
            if (currentAutoDeposit != null)
            {
                HasExistingAutoDeposit = true;
                AutoDepositAmountText = currentAutoDeposit.Amount.ToString(CultureInfo.InvariantCulture);
                AutoDepositFrequency = currentAutoDeposit.Frequency.ToString();
                AutoDepositStartDate = new DateTimeOffset(currentAutoDeposit.NextRunDate);
                AutoDepositIsActive = currentAutoDeposit.IsActive;
            }
            else
            {
                HasExistingAutoDeposit = false;
                AutoDepositAmountText = string.Empty;
                AutoDepositFrequency = string.Empty;
                AutoDepositStartDate = DateTimeOffset.Now.AddDays(1);
                AutoDepositIsActive = true;
            }
        }

        public async Task SaveAutoDepositAsync()
        {
            ErrorMessage = string.Empty;
            AutoDepositSaveMessage = string.Empty;

            if (!savingsUiRulesService.TryParsePositiveAmount(AutoDepositAmountText, out decimal amount))
            { ErrorMessage = "Auto deposit amount must be positive."; return; }

            if (string.IsNullOrWhiteSpace(AutoDepositFrequency))
            { ErrorMessage = "Please select a frequency."; return; }

            if (!savingsUiRulesService.TryParseDepositFrequency(AutoDepositFrequency, out var freq))
            { ErrorMessage = "Invalid frequency."; return; }

            var autoDeposit = new AutoDeposit
            {
                Id = currentAutoDeposit?.Id ?? 0,
                SavingsAccountId = SelectedAccount!.Id,
                Amount = amount,
                Frequency = freq,
                NextRunDate = AutoDepositStartDate?.DateTime ?? DateTime.Now.AddDays(1),
                IsActive = AutoDepositIsActive
            };

            await savingsService.SaveAutoDepositAsync(autoDeposit);
            AutoDepositSaveMessage = "Auto deposit saved successfully.";
            await LoadAutoDepositAsync(SelectedAccount.Id);
        }

        // ── Constructor ──────────────────────────────────────────────────────

        public SavingsViewModel(ISavingsService savingsService)
        {
            this.savingsService = savingsService;
            savingsUiRulesService = new SavingsUiRulesService();
            savingsPresentationService = new SavingsPresentationService();
            savingsWorkflowService = new SavingsWorkflowService();
        }

        // ── Commands: My Accounts ────────────────────────────────────────────

        [RelayCommand]
        public async Task LoadAccountsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var accountsList = await savingsService.GetAccountsAsync(CurrentUserId);
                SavingsAccounts.Clear();
                foreach (var account in accountsList) SavingsAccounts.Add(account);

                OnPropertyChanged(nameof(IsEmpty));
                OnPropertyChanged(nameof(ShowAccountsList));

                TotalSavedAmount = savingsPresentationService.BuildTotalSavedAmount(SavingsAccounts);
                NumberOfAccountsText = savingsPresentationService.BuildNumberOfAccountsText(SavingsAccounts.Count);
                BestInterestRate = savingsPresentationService.BuildBestInterestRate(SavingsAccounts);
            }
            catch (Exception exception) { ErrorMessage = exception.Message; }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        public async Task CloseAccountAsync(SavingsAccount account)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var closureResultDto = await savingsService.CloseAccountAsync(account.Id, CurrentUserId, 1);
                bool ok = closureResultDto.Success;
                if (!ok) { ErrorMessage = "Failed to close account."; return; }
                await LoadAccountsAsync();
            }
            catch (Exception exception) { ErrorMessage = exception.Message; }
            finally { IsLoading = false; }
        }

        // ── Close Account Panel ──────────────────────────────────────────────
        [ObservableProperty] private ObservableCollection<SavingsAccount> closeDestinationAccounts = new();

        private int selectedCloseDestinationId;
        public int SelectedCloseDestinationId
        {
            get => selectedCloseDestinationId;
            set { selectedCloseDestinationId = value; OnPropertyChanged(); }
        }

        private bool closeUserConfirmed;
        public bool CloseUserConfirmed
        {
            get => closeUserConfirmed;
            set { closeUserConfirmed = value; OnPropertyChanged(); }
        }

        [ObservableProperty] private string closeResultMessage = string.Empty;
        [ObservableProperty] private bool closeSuccess;

        public bool CloseHasPenalty => savingsPresentationService.HasClosePenaltyRisk(SelectedAccount);

        public async Task LoadCloseDestinationAccountsAsync()
        {
            CloseUserConfirmed = false;
            CloseResultMessage = string.Empty;
            CloseSuccess = false;
            var openAccountsList = await savingsService.GetValidTransferDestinationsAsync(SelectedAccount!.Id);
            CloseDestinationAccounts.Clear();
            foreach (var account in openAccountsList)
                CloseDestinationAccounts.Add(account);
            SelectedCloseDestinationId = savingsWorkflowService.GetDefaultCloseDestinationId(CloseDestinationAccounts);
            OnPropertyChanged(nameof(CloseHasPenalty));
        }

        public async Task<bool> ConfirmCloseAsync()
        {
            var closeValidation = savingsWorkflowService.ValidateCloseConfirmation(CloseUserConfirmed, SelectedCloseDestinationId);
            if (!closeValidation.IsValid) { CloseResultMessage = closeValidation.ErrorMessage; return false; }
            IsLoading = true;
            try
            {
                var result = await savingsService.CloseAccountAsync(SelectedAccount!.Id, SelectedCloseDestinationId, CurrentUserId);
                CloseSuccess = result.Success;
                CloseResultMessage = result.Success ? "Account closed successfully." : result.Message;
                if (result.Success) await LoadAccountsAsync();
                return result.Success;
            }
            catch (Exception exception) { CloseResultMessage = exception.Message; return false; }
            finally { IsLoading = false; }
        }

        // ── Commands: Create Account ─────────────────────────────────────────

        public async Task LoadFundingSourcesAsync()
        {
            try
            {
                var fundingSourcesList = await savingsService.GetFundingSourcesAsync(CurrentUserId);
                FundingSources.Clear();
                foreach (var fundingSource in fundingSourcesList) FundingSources.Add(fundingSource);
                SelectedFundingSource = savingsWorkflowService.GetDefaultFundingSource(FundingSources);
            }
            catch (Exception exception) { ErrorMessage = exception.Message; }
        }

        public void PrepareCreateAccountSubmission(
            string accountName,
            string initialDepositText,
            FundingSourceOption? fundingSource,
            string targetAmountText,
            DateTimeOffset? targetDate,
            DateTimeOffset? maturityDate)
        {
            AccountName = accountName;
            InitialDepositText = initialDepositText;
            SelectedFundingSource = fundingSource;
            TargetAmount = null;

            if (IsGoalSavings && savingsUiRulesService.TryParsePositiveAmount(targetAmountText, out decimal parsedTargetAmount))
            {
                TargetAmount = parsedTargetAmount;
            }

            TargetDate = IsGoalSavings ? targetDate : null;
            MaturityDate = SelectedSavingsType == "FixedDeposit" ? maturityDate : null;
        }

        [RelayCommand]
        public async Task CreateAccountAsync()
        {
            FieldErrors.Clear();
            ErrorMessage = string.Empty;
            ShowCreateConfirmation = false;

            var errors = savingsUiRulesService.ValidateCreateAccount(
                SelectedSavingsType,
                AccountName,
                InitialDepositText,
                SelectedFundingSource != null,
                SelectedFrequency,
                TargetAmount,
                TargetDate,
                IsGoalSavings);

            foreach (var error in errors)
            {
                FieldErrors[error.Key] = error.Value;
            }

            OnPropertyChanged(nameof(FieldErrors));
            if (FieldErrors.Count > 0) return;
            savingsUiRulesService.TryParsePositiveAmount(InitialDepositText, out decimal deposit);

            IsLoading = true;
            try
            {
                var createSavingsAccountDto = new CreateSavingsAccountDto
                {
                    UserId = CurrentUserId,
                    SavingsType = SelectedSavingsType,
                    AccountName = AccountName.Trim(),
                    InitialDeposit = deposit,
                    FundingAccountId = SelectedFundingSource!.Id,
                    TargetAmount = IsGoalSavings ? TargetAmount : null,
                    TargetDate = IsGoalSavings ? TargetDate?.DateTime : null,
                    MaturityDate = MaturityDate?.DateTime,
                    DepositFrequency = savingsUiRulesService.TryParseDepositFrequency(SelectedFrequency, out var selectedFrequency) ? selectedFrequency : null
                };
                await savingsService.CreateAccountAsync(createSavingsAccountDto);
                ShowCreateConfirmation = true;
                ResetCreateForm();
                await LoadAccountsAsync();
            }
            catch (Exception exception) { ErrorMessage = exception.Message; }
            finally { IsLoading = false; }
        }

        private void ResetCreateForm()
        {
            AccountName = string.Empty;
            InitialDepositText = string.Empty;
            SelectedSavingsType = string.Empty;
            TargetAmount = null;
            TargetDate = null;
            FieldErrors.Clear();
        }

        // ── Commands: Deposit ────────────────────────────────────────────────

        [RelayCommand]
        public async Task DepositAsync()
        {
            ErrorMessage = string.Empty;
            ShowDepositSuccess = false;

            if (SelectedAccount == null) { ErrorMessage = "No account selected."; return; }

            if (!savingsUiRulesService.TryParsePositiveAmount(DepositAmountText, out decimal amount))
            {
                ErrorMessage = "Please enter a valid positive amount.";
                return;
            }

            depositCancelationTokenSource?.Cancel();
            depositCancelationTokenSource = new CancellationTokenSource();

            IsLoading = true;
            try
            {
                var depositResponseDto = await savingsService.DepositAsync(
                    SelectedAccount.Id, amount, DepositSource, CurrentUserId);

                DepositSuccessMessage = $"Deposit successful! New balance: ${depositResponseDto.NewBalance:N2}";
                ShowDepositSuccess = true;
                DepositAmountText = string.Empty;
                await LoadAccountsAsync();
            }
            catch (OperationCanceledException) { }
            catch (Exception exception) { ErrorMessage = exception.Message; }
            finally { IsLoading = false; }
        }

        public void CancelDeposit() => depositCancelationTokenSource?.Cancel();

        [ObservableProperty]
        private ObservableCollection<SavingsTransaction> transactions = new();

        [ObservableProperty]
        private int currentPage = 1;

        [ObservableProperty]
        private int totalPages;

        [ObservableProperty]
        private string selectedFilter = "All";

        public async Task LoadTransactionsAsync(int accountId)
        {
            try
            {
                var result = await savingsService.GetTransactionsAsync(
                    accountId,
                    selectedFilter,
                    currentPage,
                    10);

                transactions.Clear();

                foreach (var tx in result.Items)
                    transactions.Add(tx);

                totalPages = savingsUiRulesService.CalculateTotalPages(result.TotalCount, 10);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
        public async Task NextPage(int accountId)
        {
            if (!savingsWorkflowService.CanMoveToNextPage(currentPage, totalPages)) return;

            currentPage++;
            await LoadTransactionsAsync(accountId);
        }

        public async Task PreviousPage(int accountId)
        {
            if (!savingsWorkflowService.CanMoveToPreviousPage(currentPage)) return;

            currentPage--;
            await LoadTransactionsAsync(accountId);
        }
        public async Task ChangeFilter(int accountId, string filter)
        {
            selectedFilter = filter;
            currentPage = 1;
            await LoadTransactionsAsync(accountId);
        }

        public DateTimeOffset? MaturityDate { get; set; }
    }
}
