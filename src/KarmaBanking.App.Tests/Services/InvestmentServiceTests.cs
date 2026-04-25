namespace KarmaBanking.App.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using KarmaBanking.App.Models;
    using KarmaBanking.App.Repositories.Interfaces;
    using KarmaBanking.App.Services;
    using Moq;
    using Xunit;

    public class InvestmentServiceTests
    {
        private readonly Mock<IInvestmentRepository> mockInvestmentRepository;
        private readonly InvestmentService investmentService;

        public InvestmentServiceTests()
        {
            this.mockInvestmentRepository = new Mock<IInvestmentRepository>();
            this.investmentService = new InvestmentService(this.mockInvestmentRepository.Object);
        }

        [Fact]
        public async Task ExecuteCryptoTradeAsync_CalculatesPercentageFeeCorrectly()
        {
            int portfolioId = 1;
            decimal quantity = 10m;
            decimal price = 100m;
            this.mockInvestmentRepository.Setup(investmentRepo => investmentRepo.GetPortfolio(portfolioId))
                .Returns(new Portfolio { TotalValue = 2000m, Holdings = new List<InvestmentHolding>() });

            await this.investmentService.ExecuteCryptoTradeAsync(portfolioId, "BTC", "BUY", quantity, price);

            this.mockInvestmentRepository.Verify(investmentRepo => investmentRepo.RecordCryptoTradeAsync(
                portfolioId, "BTC", "BUY", quantity, price, 15.00m, 10m, 100m), Times.Once);
        }

        [Fact]
        public async Task ExecuteCryptoTradeAsync_AppliesMinimumFee_WhenTradeIsSmall()
        {
            int portfolioId = 1;
            this.mockInvestmentRepository.Setup(investmentRepo => investmentRepo.GetPortfolio(portfolioId))
                .Returns(new Portfolio { TotalValue = 100m, Holdings = new List<InvestmentHolding>() });

            await this.investmentService.ExecuteCryptoTradeAsync(portfolioId, "BTC", "BUY", 1m, 10m);

            this.mockInvestmentRepository.Verify(investmentRepo => investmentRepo.RecordCryptoTradeAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(),
                It.IsAny<decimal>(), 0.50m, It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteCryptoTradeAsync_CalculatesWeightedAverageCorrectly_WithExistingHoldings()
        {
            int portfolioId = 1;
            var portfolio = new Portfolio { TotalValue = 500000m };
            portfolio.Holdings.Add(new InvestmentHolding
            {
                Ticker = "BTC",
                Quantity = 5m,
                AveragePurchasePrice = 20000m
            });

            this.mockInvestmentRepository.Setup(investmentRepo => investmentRepo.GetPortfolio(portfolioId)).Returns(portfolio);

            await this.investmentService.ExecuteCryptoTradeAsync(portfolioId, "BTC", "BUY", 5m, 40000m);

            this.mockInvestmentRepository.Verify(investmentRepo => investmentRepo.RecordCryptoTradeAsync(
                portfolioId, "BTC", "BUY", 5m, 40000m, It.IsAny<decimal>(), 10m, 30000m), Times.Once);
        }

        [Fact]
        public async Task ExecuteCryptoTradeAsync_ThrowsException_WhenFundsAreInsufficient()
        {
            int portfolioId = 1;
            this.mockInvestmentRepository.Setup(investmentRepo => investmentRepo.GetPortfolio(portfolioId))
                .Returns(new Portfolio { TotalValue = 10m, Holdings = new List<InvestmentHolding>() });

            await Assert.ThrowsAsync<ArgumentException>(() =>
                this.investmentService.ExecuteCryptoTradeAsync(portfolioId, "BTC", "BUY", 1m, 20m));
        }

        [Fact]
        public async Task ExecuteCryptoTradeAsync_SellOrder_ValidatesAssetQuantity()
        {
            int portfolioId = 1;
            var portfolio = new Portfolio { TotalValue = 1000m };
            portfolio.Holdings.Add(new InvestmentHolding { Ticker = "BTC", Quantity = 5m });
            this.mockInvestmentRepository.Setup(investmentRepo => investmentRepo.GetPortfolio(portfolioId)).Returns(portfolio);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this.investmentService.ExecuteCryptoTradeAsync(portfolioId, "BTC", "SELL", 10m, 50000m));
        }

        [Fact]
        public async Task ExecuteCryptoTradeAsync_SellOrder_SucceedsWithSufficientAssets()
        {
            int portfolioId = 1;
            var portfolio = new Portfolio { TotalValue = 1000m };
            portfolio.Holdings.Add(new InvestmentHolding { Ticker = "BTC", Quantity = 10m, AveragePurchasePrice = 20000m });
            this.mockInvestmentRepository.Setup(investmentRepo => investmentRepo.GetPortfolio(portfolioId)).Returns(portfolio);

            var result = await this.investmentService.ExecuteCryptoTradeAsync(portfolioId, "BTC", "SELL", 1m, 50000m);

            Assert.True(result);
            this.mockInvestmentRepository.Verify(investmentRepo => investmentRepo.RecordCryptoTradeAsync(
                portfolioId, "BTC", "SELL", 1m, 50000m, It.IsAny<decimal>(), 9m, 20000m), Times.Once);
        }

        [Fact]
        public async Task ExecuteCryptoTradeAsync_ThrowsWrappedException_WhenRepositoryFails()
        {
            int portfolioId = 1;
            this.mockInvestmentRepository.Setup(investmentRepo => investmentRepo.GetPortfolio(portfolioId))
                .Returns(new Portfolio { TotalValue = 1000m, Holdings = new List<InvestmentHolding>() });

            this.mockInvestmentRepository.Setup(investmentRepo => investmentRepo.RecordCryptoTradeAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(),
                It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
                .ThrowsAsync(new Exception("DB Failure"));

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                this.investmentService.ExecuteCryptoTradeAsync(portfolioId, "BTC", "BUY", 1m, 100m));

            Assert.Contains("Trade execution failed", exception.Message);
        }

        [Fact]
        public void GetPortfolio_ReturnsPortfolioFromRepository()
        {
            int userId = 123;
            var expectedPortfolio = new Portfolio { IdentificationNumber = 1, TotalValue = 500m };
            this.mockInvestmentRepository.Setup(investmentRepo => investmentRepo.GetPortfolio(userId)).Returns(expectedPortfolio);

            var result = this.investmentService.GetPortfolio(userId);

            Assert.Equal(expectedPortfolio.IdentificationNumber, result.IdentificationNumber);
        }

        [Fact]
        public async Task GetInvestmentLogsAsync_ThrowsOnInvalidPortfolioId()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => this.investmentService.GetInvestmentLogsAsync(0));
        }

        [Fact]
        public async Task GetInvestmentLogsAsync_ThrowsWhenStartDateAfterEndDate()
        {
            DateTime start = DateTime.Now;
            DateTime end = start.AddDays(-1);
            await Assert.ThrowsAsync<ArgumentException>(() => this.investmentService.GetInvestmentLogsAsync(1, start, end));
        }

        [Fact]
        public async Task ExecuteCryptoTradeAsync_ValidatesInputs_ThrowsOnInvalidValues()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => this.investmentService.ExecuteCryptoTradeAsync(1, string.Empty, "BUY", 1, 100));
            await Assert.ThrowsAsync<ArgumentException>(() => this.investmentService.ExecuteCryptoTradeAsync(1, "BTC", "BUY", 0, 100));
            await Assert.ThrowsAsync<ArgumentException>(() => this.investmentService.ExecuteCryptoTradeAsync(1, "BTC", "BUY", 1, -1));
            await Assert.ThrowsAsync<ArgumentException>(() => this.investmentService.ExecuteCryptoTradeAsync(1, "BTC", "INVALID", 1, 100));
        }

        [Fact]
        public async Task GetInvestmentLogsAsync_ReturnsDataFromRepository()
        {
            int portfolioId = 1;
            var expectedLogs = new List<InvestmentTransaction>
            {
                new InvestmentTransaction { Ticker = "BTC", Quantity = 1.0m, PricePerUnit = 45000m }
            };
            this.mockInvestmentRepository.Setup(investmentRepo => investmentRepo.GetInvestmentLogsAsync(portfolioId, null, null, null)).ReturnsAsync(expectedLogs);

            var result = await this.investmentService.GetInvestmentLogsAsync(portfolioId);

            Assert.Single(result);
            Assert.Equal("BTC", result[0].Ticker);
        }
    }
}