using System.Collections.Generic;
using System.Data.SqlClient;

public interface ILoanRepository
{
    List<Loan> GetAllLoans();

    List<Loan> GetLoansByUser(int userId);

    List<Loan> GetLoansByStatus(string loanStatus);

    List<Loan> GetLoansByType(string loanType);

}