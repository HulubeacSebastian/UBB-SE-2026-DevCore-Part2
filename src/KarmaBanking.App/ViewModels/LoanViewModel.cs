using CommunityToolkit.Mvvm.ComponentModel;
using KarmaBanking.App.Services;

namespace KarmaBanking.App.ViewModels
{
    public partial class LoanViewModel : ObservableObject
    {
        private readonly Loan _loan;
        private readonly LoanPresentationService _loanPresentationService;

        public Loan Loan => _loan;

        public double RepaymentProgress =>
           _loanPresentationService.GetRepaymentProgress(_loan);

        public int PaidInstallments => _loan.TermInMonths - _loan.RemainingMonths;

        public LoanViewModel(Loan loan)
        {
            _loan = loan;
            _loanPresentationService = new LoanPresentationService();
        }

    }
}
