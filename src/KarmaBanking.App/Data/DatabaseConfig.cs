using Microsoft.Data.SqlClient;

namespace KarmaBanking.App.Data
{
    public static class DatabaseConfig
    {
        private static readonly string ConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=KarmaBankingDb;Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection GetDatabaseConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
