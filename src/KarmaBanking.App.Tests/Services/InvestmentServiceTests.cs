// <copyright file="InvestmentServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

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
        private readonly Mock<IInvestmentRepository> investmentRepositoryMock;
        private readonly InvestmentService investmentService;

        public InvestmentServiceTests()
        {
            this.investmentRepositoryMock = new Mock<IInvestmentRepository>();
            this.investmentService = new InvestmentService(this.investmentRepositoryMock.Object);
        }

        [Fact]
        public async Task ExecuteCryptoTradeAsync_CalculatesWeightedAverageCorrectly_WithExistingHoldings()
        {
            // Arrange
            int portfolioIdentificationNumber = 1;
            var userPortfolio = new Portfolio { TotalValue = 500000m };
            userPortfolio.Holdings.Add(new InvestmentHolding
            {
                Ticker = "BTC",
                Quantity = 5m,
                AveragePurchasePrice = 20000m
            });

            this.investmentRepositoryMock.Setup(repository => repository.GetPortfolio(portfolioIdentificationNumber))
                .Returns(userPortfolio);

            // Act
            await this.investmentService.ExecuteCryptoTradeAsync(portfolioIdentificationNumber, "BTC", "BUY", 5m, 40000m);

            // Assert
            this.investmentRepositoryMock.Verify(repository => repository.RecordCryptoTradeAsync(
                portfolioIdentificationNumber, "BTC", "BUY", 5m, 40000m, It.IsAny<decimal>(), 10m, 30000m), Times.Once);
        }

        [Fact]
        public async Task ExecuteCryptoTradeAsync_ThrowsWrappedException_WhenRepositoryFails()
        {
            // Arrange
            int portfolioIdentificationNumber = 1;
            this.investmentRepositoryMock.Setup(repository => repository.GetPortfolio(portfolioIdentificationNumber))
                .Returns(new Portfolio { TotalValue = 1000m, Holdings = new List<InvestmentHolding>() });

            this.investmentRepositoryMock.Setup(repository => repository.RecordCryptoTradeAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(),
                It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
                .ThrowsAsync(new Exception("Database Failure"));

            // Act & Assert
            var validationException = await Assert.ThrowsAsync<Exception>(() =>
                this.investmentService.ExecuteCryptoTradeAsync(portfolioIdentificationNumber, "BTC", "BUY", 1m, 100m));

            Assert.Contains("Trade execution failed", validationException.Message);
        }

        [Fact]
        public void GetPortfolio_ReturnsPortfolioFromRepository()
        {
            // Arrange
            int userIdentificationNumber = 123;
            var expectedPortfolioInstance = new Portfolio { IdentificationNumber = 1, TotalValue = 500m };
            this.investmentRepositoryMock.Setup(repository => repository.GetPortfolio(userIdentificationNumber)).Returns(expectedPortfolioInstance);

            // Act
            var actualPortfolioResult = this.investmentService.GetPortfolio(userIdentificationNumber);

            // Assert
            Assert.Equal(expectedPortfolioInstance.IdentificationNumber, actualPortfolioResult.IdentificationNumber);
        }

        [Fact]
        public async Task GetInvestmentLogsAsync_ThrowsWhenStartDateAfterEndDate()
        {
            // Arrange
            DateTime startDateTimeValue = DateTime.Now;
            DateTime endDateTimeValue = startDateTimeValue.AddDays(-1);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => this.investmentService.GetInvestmentLogsAsync(1, startDateTimeValue, endDateTimeValue));
        }
    }
}