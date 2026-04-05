using KarmaBanking.App.Models;
using KarmaBanking.App.Models.DTOs;
using KarmaBanking.App.Services;
using KarmaBanking.App.Services.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;

namespace KarmaBanking.App.ViewModels
{
    public class CloseAccountViewModel : BaseViewModel
    {
        private readonly ISavingsService _service;
        private const int CurrentUserId = 1;

        public SavingsAccount Account { get; }

        public ObservableCollection<SavingsAccount> DestinationAccounts { get; } = new();

        public int SelectedDestinationAccountId { get; set; }

        public bool UserConfirmed { get; set; }

        public CloseAccountViewModel(ISavingsService service, SavingsAccount account)
        {
            _service = service;
            Account = account;

            OnPropertyChanged(nameof(HasPenalty));
        }

        public async Task LoadAccountsAsync()
        {
            var accounts = await _service.GetAccountsAsync(CurrentUserId);

            DestinationAccounts.Clear();

            foreach (var acc in accounts.Where(a =>
            a.Id != Account.Id &&
            a.AccountStatus != "Closed"))
            {
                DestinationAccounts.Add(acc);
            }

            // auto-select first if exists
            if (DestinationAccounts.Count > 0)
                SelectedDestinationAccountId = DestinationAccounts[0].Id;
        }

        public async Task<ClosureResult> CloseAsync()
        {
            if (!UserConfirmed)
            {
                return new ClosureResult
                {
                    Success = false,
                    Message = "Please confirm account closure."
                };
            }

            if (SelectedDestinationAccountId == 0)
            {
                return new ClosureResult
                {
                    Success = false,
                    Message = "Please select a destination account."
                };
            }

            return await _service.CloseAccountAsync(
                Account.Id,
                SelectedDestinationAccountId,
                CurrentUserId
            );
        }

        public decimal EstimatedPenalty =>
        Account.SavingsType == "FixedDeposit" &&
        Account.MaturityDate.HasValue &&
        Account.MaturityDate > DateTime.UtcNow
            ? Account.Balance * 0.02m
            : 0;

        public decimal EstimatedTransfer => Account.Balance - EstimatedPenalty;
        public bool HasPenalty => EstimatedPenalty > 0;
    }
}