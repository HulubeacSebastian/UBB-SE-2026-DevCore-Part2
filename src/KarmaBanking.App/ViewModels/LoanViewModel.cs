namespace KarmaBanking.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using KarmaBanking.App.Services;

public partial class LoanViewModel : ObservableObject
{
    private readonly LoanPresentationService loanPresentationService;

    public LoanViewModel(Loan loan)
    {
        this.Loan = loan;
        this.loanPresentationService = new LoanPresentationService();
    }

    public Loan Loan { get; }

    public double RepaymentProgress =>
        this.loanPresentationService.GetRepaymentProgress(this.Loan);

    public int PaidInstallments => this.Loan.TermInMonths - this.Loan.RemainingMonths;
}