using Microsoft.Data.SqlClient;

public static class DatabaseConfig
{
    //public static readonly string ConnectionString =
    //    "Server=(localdb)\\MSSQLLocalDB;Database=KarmaBankingDb;Trusted_Connection=True;TrustServerCertificate=True;";

    public static readonly string ConnectionString =
<<<<<<< HEAD
        "Server=DESKTOP-FNDDLP7\\SQLEXPRESS;Database=KarmaBankingDb;Trusted_Connection=True;TrustServerCertificate=True;";
=======
    "Server=localhost;Database=KarmaBankingDb;Trusted_Connection=True;TrustServerCertificate=True;";
>>>>>>> feature/savings

    public static SqlConnection GetDatabaseConnection()
    {
        return new SqlConnection(ConnectionString);
    }
}