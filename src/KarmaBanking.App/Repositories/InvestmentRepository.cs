namespace KarmaBanking.App.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using KarmaBanking.App.Data;
    using KarmaBanking.App.Models;
    using KarmaBanking.App.Repositories.Interfaces;
    using Microsoft.Data.SqlClient;

    public class InvestmentRepository : IInvestmentRepository
    {
        private const string AssetTypeCrypto = "Crypto";
        private const string OrderTypeMarket = "Market";
        private const decimal InitialUnrealizedGainLoss = 0m;

        /// <summary>
        /// Records a crypto trade and updates the holding with final values calculated by the Service.
        /// </summary>
        public async Task RecordCryptoTradeAsync(
            int portfolioIdentificationNumber,
            string ticker,
            string actionType,
            decimal quantity,
            decimal pricePerUnit,
            decimal fees,
            decimal finalQuantity,
            decimal finalAveragePrice)
        {
            using var sqlConnection = DatabaseConfig.GetDatabaseConnection();
            await sqlConnection.OpenAsync();

            using var sqlTransaction = (SqlTransaction)await sqlConnection.BeginTransactionAsync();

            try
            {
                int? holdingIdentificationNumber = null;

                // First step: check if the holding already exists.
                var checkHoldingSqlQuery = "SELECT id FROM InvestmentHolding WHERE portfolioId = @PortfolioId AND ticker = @Ticker";
                using (var checkCommand = new SqlCommand(checkHoldingSqlQuery, sqlConnection, sqlTransaction))
                {
                    checkCommand.Parameters.AddWithValue("@PortfolioId", portfolioIdentificationNumber);
                    checkCommand.Parameters.AddWithValue("@Ticker", ticker);

                    var result = await checkCommand.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        holdingIdentificationNumber = (int)result!;
                    }
                }

                // Second step: update existing holding or insert a new holding with final values from the service.
                if (holdingIdentificationNumber.HasValue)
                {
                    var updateHoldingSqlQuery = "UPDATE InvestmentHolding SET quantity = @FinalQuantity, avgPurchasePrice = @FinalAveragePrice WHERE id = @HoldingId";
                    using var updateCommand = new SqlCommand(updateHoldingSqlQuery, sqlConnection, sqlTransaction);
                    updateCommand.Parameters.AddWithValue("@FinalQuantity", finalQuantity);
                    updateCommand.Parameters.AddWithValue("@FinalAveragePrice", finalAveragePrice);
                    updateCommand.Parameters.AddWithValue("@HoldingId", holdingIdentificationNumber.Value);

                    await updateCommand.ExecuteNonQueryAsync();
                }
                else
                {
                    var insertHoldingSqlQuery = @"
                            INSERT INTO InvestmentHolding (portfolioId, ticker, assetType, quantity, avgPurchasePrice, currentPrice, unrealizedGainLoss)
                            OUTPUT INSERTED.id
                            VALUES (@PortfolioId, @Ticker, @AssetType, @Quantity, @AveragePrice, @AveragePrice, @UnrealizedGainLoss)";

                    using var insertCommand = new SqlCommand(insertHoldingSqlQuery, sqlConnection, sqlTransaction);
                    insertCommand.Parameters.AddWithValue("@PortfolioId", portfolioIdentificationNumber);
                    insertCommand.Parameters.AddWithValue("@Ticker", ticker);
                    insertCommand.Parameters.AddWithValue("@AssetType", AssetTypeCrypto);
                    insertCommand.Parameters.AddWithValue("@Quantity", finalQuantity);
                    insertCommand.Parameters.AddWithValue("@AveragePrice", finalAveragePrice);
                    insertCommand.Parameters.AddWithValue("@UnrealizedGainLoss", InitialUnrealizedGainLoss);

                    holdingIdentificationNumber = (int)(await insertCommand.ExecuteScalarAsync())!;
                }

                // Third step: write the transaction log entry.
                var insertTransactionSqlQuery = @"
                        INSERT INTO InvestmentTransaction (holdingId, ticker, actionType, quantity, pricePerUnit, fees, orderType, executedAt)
                        VALUES (@HoldingId, @Ticker, @ActionType, @Quantity, @PricePerUnit, @Fees, @OrderType, @ExecutedAt)";

                using var transactionLogCommand = new SqlCommand(insertTransactionSqlQuery, sqlConnection, sqlTransaction);
                transactionLogCommand.Parameters.AddWithValue("@HoldingId", holdingIdentificationNumber);
                transactionLogCommand.Parameters.AddWithValue("@Ticker", ticker);
                transactionLogCommand.Parameters.AddWithValue("@ActionType", actionType.ToUpper());
                transactionLogCommand.Parameters.AddWithValue("@Quantity", quantity);
                transactionLogCommand.Parameters.AddWithValue("@PricePerUnit", pricePerUnit);
                transactionLogCommand.Parameters.AddWithValue("@Fees", fees);
                transactionLogCommand.Parameters.AddWithValue("@OrderType", OrderTypeMarket);
                transactionLogCommand.Parameters.AddWithValue("@ExecutedAt", DateTime.Now);

                await transactionLogCommand.ExecuteNonQueryAsync();

                await sqlTransaction.CommitAsync();
            }
            catch (Exception)
            {
                await sqlTransaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a user's portfolio and associated holdings.
        /// </summary>
        public Portfolio GetPortfolio(int userIdentificationNumber)
        {
            const string selectPortfolioSqlQuery = "SELECT id, userId, totalValue, totalGainLoss, gainLossPercent FROM Portfolio WHERE userId = @UserId";
            const string selectHoldingsSqlQuery = "SELECT id, ticker, assetType, quantity, avgPurchasePrice, currentPrice, unrealizedGainLoss FROM InvestmentHolding WHERE portfolioId = @PortfolioId ORDER BY id";

            var userPortfolio = new Portfolio { UserIdentificationNumber = userIdentificationNumber };

            using var sqlConnection = new SqlConnection(DatabaseConfig.DatabaseConnectionString);
            sqlConnection.Open();

            using (var selectPortfolioCommand = new SqlCommand(selectPortfolioSqlQuery, sqlConnection))
            {
                selectPortfolioCommand.Parameters.Add("@UserId", SqlDbType.Int).Value = userIdentificationNumber;
                using var portfolioDataReader = selectPortfolioCommand.ExecuteReader();
                if (portfolioDataReader.Read())
                {
                    var portfolioIdOrdinal = portfolioDataReader.GetOrdinal("id");
                    var totalValueOrdinal = portfolioDataReader.GetOrdinal("totalValue");
                    var totalGainLossOrdinal = portfolioDataReader.GetOrdinal("totalGainLoss");
                    var gainLossPercentOrdinal = portfolioDataReader.GetOrdinal("gainLossPercent");

                    userPortfolio.IdentificationNumber = portfolioDataReader.GetInt32(portfolioIdOrdinal);
                    userPortfolio.TotalValue = portfolioDataReader.GetDecimal(totalValueOrdinal);
                    userPortfolio.TotalGainLoss = portfolioDataReader.GetDecimal(totalGainLossOrdinal);
                    userPortfolio.GainLossPercent = portfolioDataReader.GetDecimal(gainLossPercentOrdinal);
                }
                else
                {
                    return userPortfolio;
                }
            }

            using (var selectHoldingsCommand = new SqlCommand(selectHoldingsSqlQuery, sqlConnection))
            {
                selectHoldingsCommand.Parameters.Add("@PortfolioId", SqlDbType.Int).Value = userPortfolio.IdentificationNumber;
                using var holdingsDataReader = selectHoldingsCommand.ExecuteReader();
                while (holdingsDataReader.Read())
                {
                    var holdingIdOrdinal = holdingsDataReader.GetOrdinal("id");
                    var tickerOrdinal = holdingsDataReader.GetOrdinal("ticker");
                    var assetTypeOrdinal = holdingsDataReader.GetOrdinal("assetType");
                    var quantityOrdinal = holdingsDataReader.GetOrdinal("quantity");
                    var averagePurchasePriceOrdinal = holdingsDataReader.GetOrdinal("avgPurchasePrice");
                    var currentPriceOrdinal = holdingsDataReader.GetOrdinal("currentPrice");
                    var unrealizedGainLossOrdinal = holdingsDataReader.GetOrdinal("unrealizedGainLoss");

                    userPortfolio.Holdings.Add(new InvestmentHolding
                    {
                        IdentificationNumber = holdingsDataReader.GetInt32(holdingIdOrdinal),
                        Ticker = holdingsDataReader.IsDBNull(tickerOrdinal) ? string.Empty : holdingsDataReader.GetString(tickerOrdinal),
                        AssetType = holdingsDataReader.IsDBNull(assetTypeOrdinal) ? string.Empty : holdingsDataReader.GetString(assetTypeOrdinal),
                        Quantity = holdingsDataReader.GetDecimal(quantityOrdinal),
                        AveragePurchasePrice = holdingsDataReader.GetDecimal(averagePurchasePriceOrdinal),
                        CurrentPrice = holdingsDataReader.GetDecimal(currentPriceOrdinal),
                        UnrealizedGainLoss = holdingsDataReader.GetDecimal(unrealizedGainLossOrdinal)
                    });
                }
            }

            return userPortfolio;
        }

        /// <summary>
        /// Retrieves transaction logs with optional filtering.
        /// </summary>
        public async Task<List<InvestmentTransaction>> GetInvestmentLogsAsync(
            int portfolioIdentificationNumber,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? ticker = null)
        {
            var investmentLogs = new List<InvestmentTransaction>();

            using var sqlConnection = DatabaseConfig.GetDatabaseConnection();
            await sqlConnection.OpenAsync();

            var filterLogsSqlQuery = @"
                    SELECT t.id, t.holdingId, t.ticker, t.actionType, t.quantity, t.pricePerUnit, t.fees, t.orderType, t.executedAt 
                    FROM InvestmentTransaction t
                    INNER JOIN InvestmentHolding h ON t.holdingId = h.id
                    WHERE h.portfolioId = @PortfolioId";

            if (startDate.HasValue)
            {
                filterLogsSqlQuery += " AND t.executedAt >= @StartDate";
            }
            if (endDate.HasValue)
            {
                filterLogsSqlQuery += " AND t.executedAt <= @EndDate";
            }
            if (!string.IsNullOrWhiteSpace(ticker))
            {
                filterLogsSqlQuery += " AND t.ticker = @Ticker";
            }

            filterLogsSqlQuery += " ORDER BY t.executedAt DESC";

            using var filterCommand = new SqlCommand(filterLogsSqlQuery, sqlConnection);
            filterCommand.Parameters.AddWithValue("@PortfolioId", portfolioIdentificationNumber);
            if (startDate.HasValue)
            {
                filterCommand.Parameters.AddWithValue("@StartDate", startDate.Value);
            }
            if (endDate.HasValue)
            {
                filterCommand.Parameters.AddWithValue("@EndDate", endDate.Value);
            }
            if (!string.IsNullOrWhiteSpace(ticker))
            {
                filterCommand.Parameters.AddWithValue("@Ticker", ticker);
            }

            using var transactionLogDataReader = await filterCommand.ExecuteReaderAsync();
            while (await transactionLogDataReader.ReadAsync())
            {
                var transactionIdOrdinal = transactionLogDataReader.GetOrdinal("id");
                var holdingIdOrdinal = transactionLogDataReader.GetOrdinal("holdingId");
                var tickerOrdinal = transactionLogDataReader.GetOrdinal("ticker");
                var actionTypeOrdinal = transactionLogDataReader.GetOrdinal("actionType");
                var quantityOrdinal = transactionLogDataReader.GetOrdinal("quantity");
                var pricePerUnitOrdinal = transactionLogDataReader.GetOrdinal("pricePerUnit");
                var feesOrdinal = transactionLogDataReader.GetOrdinal("fees");
                var orderTypeOrdinal = transactionLogDataReader.GetOrdinal("orderType");
                var executedAtOrdinal = transactionLogDataReader.GetOrdinal("executedAt");

                investmentLogs.Add(new InvestmentTransaction
                {
                    IdentificationNumber = transactionLogDataReader.GetInt32(transactionIdOrdinal),
                    HoldingIdentificationNumber = transactionLogDataReader.GetInt32(holdingIdOrdinal),
                    Ticker = transactionLogDataReader.GetString(tickerOrdinal),
                    ActionType = transactionLogDataReader.GetString(actionTypeOrdinal),
                    Quantity = transactionLogDataReader.GetDecimal(quantityOrdinal),
                    PricePerUnit = transactionLogDataReader.GetDecimal(pricePerUnitOrdinal),
                    Fees = transactionLogDataReader.GetDecimal(feesOrdinal),
                    OrderType = transactionLogDataReader.GetString(orderTypeOrdinal),
                    ExecutedAt = transactionLogDataReader.GetDateTime(executedAtOrdinal)
                });
            }

            return investmentLogs;
        }
    }
}