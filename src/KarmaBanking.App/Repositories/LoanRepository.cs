
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Data;

public class LoanRepository : ILoanRepository
{
    public List<Loan> GetAllLoans()
    {
        List<Loan> loans = new List<Loan>();
        using (SqlConnection connection = new SqlConnection(DatabaseConfig.connectionString))
        {
            connection.Open();

            string query = "SELECT * FROM Loans";

            SqlCommand command = new SqlCommand(query, connection);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {

                Loan loan = ReaderToLoan(reader);

                loans.Add(loan);
            }

        }
        return loans;
    }
    public List<Loan> GetLoansByUser(int userId)
    {
        List<Loan> loans = new List<Loan>();

        using (SqlConnection connection = new SqlConnection(DatabaseConfig.connectionString))
        {
            connection.Open();

            string query = "SELECT * FROM Loans l WHERE l.userId = @userId";

            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {

                Loan loan = ReaderToLoan(reader);

                loans.Add(loan);
            }
            return loans;
        }



    }
    public List<Loan> GetLoansByType(string loanType)
    {
        List<Loan> loans = new List<Loan>();

        using (SqlConnection connection = new SqlConnection(DatabaseConfig.connectionString))
        {
            connection.Open();

            string query = "SELECT * FROM Loans l WHERE l.loanType = @loanType";

            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.Add("@loanType", SqlDbType.NVarChar, 50).Value = loanType;
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Loan loan = ReaderToLoan(reader);

                loans.Add(loan);
            }
            return loans;
        }



    }

    public List<Loan> GetLoansByStatus(string loanStatus)
    {
        List<Loan> loans = new List<Loan>();

        using (SqlConnection connection = new SqlConnection(DatabaseConfig.connectionString))
        {
            connection.Open();

            string query = "SELECT * FROM Loans l WHERE l.loanStatus = @loanStatus";

            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.Add("@loanStatus", SqlDbType.NVarChar, 50).Value = loanStatus;
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Loan loan = ReaderToLoan(reader);

                loans.Add(loan);
            }
            return loans;
        }



    }
    private Loan ReaderToLoan(SqlDataReader reader)
    {
        return new Loan
        {
            id = (int)reader["id"],
            userId = (int)reader["userId"],
            loanType = reader["loanType"].ToString(),
            principal = (decimal)reader["principal"],
            outstandingBalance = (decimal)reader["outstandingBalance"],
            interestRate = (decimal)reader["interestRate"],
            monthlyInstallment = (decimal)reader["monthlyInstallment"],
            remainingMonths = (int)reader["remainingMonths"],
            loanStatus = reader["loanStatus"].ToString()
        };
    }
}