using System.Collections.Generic;
using System.Threading.Tasks;
public interface ILoanService
{
    Task<List<Loan>> GetAllLoansAsync();

    Task<Loan> GetLoanByIdAsync(int id);

    Task<List<Loan>> GetLoansByUserAsync(int userId);

    Task<List<Loan>> GetLoansByStatusAsync(LoanStatus loanStatus);

    Task<List<Loan>> GetLoansByTypeAsync(LoanType loanType);

    Task ProcessApplicationStatus(LoanApplication application);

    decimal GetInterestRateForType(LoanType loanType);

    Task<LoanApplication> ApplyForLoanAsync(LoanApplicationRequest request);

    LoanEstimate GetLoanEstimate(LoanApplicationRequest request);

    Task<int> AddLoanAsync(LoanApplication application);

    Task PayInstallmentAsync(int loanId, decimal? amount = null);

    Task<List<AmortizationRow>> GetAmortizationAsync(int loanId);
}