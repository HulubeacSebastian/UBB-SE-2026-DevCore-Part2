using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KarmaBanking.App.Models;
using KarmaBanking.App.Models.DTOs;
using KarmaBanking.App.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace KarmaBanking.App.Repositories
{
    public class SavingsRepository : ISavingsRepository
    {
        public SavingsRepository() { }
        public async Task<List<SavingsAccount>> GetByUserIdAsync(int userId, bool includesClosed = false)
        {
            string query = @"
                SELECT id, userId, savingsType, balance, accruedInterest, apy,
                       maturityDate, accountStatus, createdAt,
                       accountName, fundingAccountId, targetAmount, targetDate
                FROM SavingsAccount
                WHERE userId = @UserId"
                + (includesClosed ? "" : " AND accountStatus != 'Closed'") +
                " ORDER BY balance DESC";

            var accounts = new List<SavingsAccount>();

            using SqlConnection conn = DatabaseConfig.GetDatabaseConnection();
            await conn.OpenAsync();

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                accounts.Add(MapReaderToAccount(reader));

            return accounts;
        }

        public async Task<SavingsAccount> CreateAsync(CreateSavingsAccountDto dto)
        {
            decimal apy = dto.SavingsType switch
            {
                "FixedDeposit" => 0.04m,
                "GoalSavings"  => 0.03m,
                "HighYield"    => 0.03m,
                _              => 0.02m
            };

            const string query = @"
                INSERT INTO SavingsAccount
                    (userId, savingsType, balance, accruedInterest, apy,
                     accountStatus, createdAt, accountName,
                     fundingAccountId, targetAmount, targetDate)
                OUTPUT INSERTED.id
                VALUES
                    (@UserId, @SavingsType, @Balance, 0, @Apy,
                     'Active', @CreatedAt, @AccountName,
                     @FundingAccountId, @TargetAmount, @TargetDate)";

            using SqlConnection conn = DatabaseConfig.GetDatabaseConnection();
            await conn.OpenAsync();

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", dto.UserId);
            cmd.Parameters.AddWithValue("@SavingsType", dto.SavingsType);
            cmd.Parameters.AddWithValue("@Balance", dto.InitialDeposit);
            cmd.Parameters.AddWithValue("@Apy", apy);
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            cmd.Parameters.AddWithValue("@AccountName", (object?)dto.AccountName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FundingAccountId", dto.FundingAccountId == 0 ? (object)DBNull.Value : dto.FundingAccountId);
            cmd.Parameters.AddWithValue("@TargetAmount", (object?)dto.TargetAmount ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TargetDate", (object?)dto.TargetDate ?? DBNull.Value);

            int newId = (int)await cmd.ExecuteScalarAsync();

            return new SavingsAccount
            {
                Id = newId,
                UserId = dto.UserId,
                SavingsType = dto.SavingsType,
                AccountName = dto.AccountName,
                Balance = dto.InitialDeposit,
                AccruedInterest = 0,
                Apy = apy,
                AccountStatus = "Active",
                CreatedAt = DateTime.Now,
                FundingAccountId = dto.FundingAccountId == 0 ? (int?)null : dto.FundingAccountId,
                TargetAmount = dto.TargetAmount,
                TargetDate = dto.TargetDate
            };
        }

        public async Task<DepositResponseDto> DepositAsync(int accountId, decimal amount, string source)
        {
            using SqlConnection conn = DatabaseConfig.GetDatabaseConnection();
            await conn.OpenAsync();
            using SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                const string updateQuery = @"
                    UPDATE SavingsAccount
                    SET balance = balance + @Amount
                    WHERE id = @AccountId";

                using SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction);
                updateCmd.Parameters.AddWithValue("@Amount", amount);
                updateCmd.Parameters.AddWithValue("@AccountId", accountId);
                await updateCmd.ExecuteNonQueryAsync();

                const string insertTxQuery = @"
                    INSERT INTO SavingsTransaction (savingsAccountId, amount, type, source, createdAt)
                    OUTPUT INSERTED.id
                    VALUES (@AccountId, @Amount, 'Deposit', @Source, @Now)";

                using SqlCommand insertCmd = new SqlCommand(insertTxQuery, conn, transaction);
                insertCmd.Parameters.AddWithValue("@AccountId", accountId);
                insertCmd.Parameters.AddWithValue("@Amount", amount);
                insertCmd.Parameters.AddWithValue("@Source", source ?? string.Empty);
                insertCmd.Parameters.AddWithValue("@Now", DateTime.Now);
                int txId = (int)await insertCmd.ExecuteScalarAsync();

                const string balanceQuery = "SELECT balance FROM SavingsAccount WHERE id = @AccountId";
                using SqlCommand balCmd = new SqlCommand(balanceQuery, conn, transaction);
                balCmd.Parameters.AddWithValue("@AccountId", accountId);
                decimal newBalance = (decimal)await balCmd.ExecuteScalarAsync();

                await transaction.CommitAsync();

                return new DepositResponseDto
                {
                    NewBalance = newBalance,
                    TransactionId = txId,
                    Timestamp = DateTime.Now
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> CloseAsync(int accountId)
        {
            const string query = @"
                UPDATE SavingsAccount
                SET accountStatus = 'Closed', balance = 0
                WHERE id = @AccountId";

            using SqlConnection conn = DatabaseConfig.GetDatabaseConnection();
            await conn.OpenAsync();

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@AccountId", accountId);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public Task<List<FundingSourceOption>> GetFundingSourcesAsync(int userId)
        {
            return Task.FromResult(new List<FundingSourceOption>
            {
                new FundingSourceOption { Id = 1, DisplayName = "Checking Account ****1234" },
                new FundingSourceOption { Id = 2, DisplayName = "Checking Account ****5678" }
            });
        }

        private static SavingsAccount MapReaderToAccount(SqlDataReader r)
        {
            return new SavingsAccount
            {
                Id               = r.GetInt32(r.GetOrdinal("id")),
                UserId           = r.GetInt32(r.GetOrdinal("userId")),
                SavingsType      = r["savingsType"]?.ToString() ?? string.Empty,
                Balance          = r.GetDecimal(r.GetOrdinal("balance")),
                AccruedInterest  = r.GetDecimal(r.GetOrdinal("accruedInterest")),
                Apy              = r.GetDecimal(r.GetOrdinal("apy")),
                MaturityDate     = r["maturityDate"] as DateTime?,
                AccountStatus    = r["accountStatus"]?.ToString() ?? string.Empty,
                CreatedAt        = r.GetDateTime(r.GetOrdinal("createdAt")),
                AccountName      = r["accountName"] as string,
                FundingAccountId = r["fundingAccountId"] as int?,
                TargetAmount     = r["targetAmount"] as decimal?,
                TargetDate       = r["targetDate"] as DateTime?
            };
        }
    }
}
