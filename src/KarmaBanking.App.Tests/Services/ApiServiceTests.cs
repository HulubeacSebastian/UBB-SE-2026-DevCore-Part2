// <copyright file="ApiServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::KarmaBanking.App.Repositories.Interfaces;
    using global::KarmaBanking.App.Services;
    using Moq;
    using Xunit;

    public class ApiServiceTests
    {
        private readonly Mock<ILoanService> loanServiceMock;
        private readonly Mock<IChatRepository> chatRepositoryMock;
        private readonly ApiService apiService;

        public ApiServiceTests()
        {
            this.loanServiceMock = new Mock<ILoanService>();
            this.chatRepositoryMock = new Mock<IChatRepository>();
            this.apiService = new ApiService(this.loanServiceMock.Object, this.chatRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllLoansAsync_WhenCalled_ThenCallsLoanServiceAndReturnsLoansList()
        {
            // Arrange
            var expectedLoansList = new List<Loan> { new Loan { Id = 1 } };
            this.loanServiceMock.Setup(service => service.GetAllLoansAsync())
                .ReturnsAsync(expectedLoansList);

            // Act
            var actualLoansResult = await this.apiService.GetAllLoansAsync();

            // Assert
            Assert.Equal(expectedLoansList, actualLoansResult);
            this.loanServiceMock.Verify(service => service.GetAllLoansAsync(), Times.Once);
        }

        [Fact]
        public async Task ApplyForLoanAsync_WhenApplicationRejected_ThenReturnsRejectionReason()
        {
            // Arrange
            var loanApplicationRequestInstance = new LoanApplicationRequest
            {
                DesiredAmount = 5000,
                Purpose = "Home Renovation"
            };

            this.loanServiceMock.Setup(service => service.SubmitLoanApplicationAsync(loanApplicationRequestInstance))
                .ReturnsAsync((LoanApplicationStatus.Rejected, "Credit score too low"));

            // Act
            string? rejectionReasonMessage = await this.apiService.ApplyForLoanAsync(loanApplicationRequestInstance);

            // Assert
            Assert.Equal("Credit score too low", rejectionReasonMessage);
            this.loanServiceMock.Verify(service => service.SubmitLoanApplicationAsync(loanApplicationRequestInstance), Times.Once);
        }

        [Fact]
        public void SubmitFeedback_WhenCalled_ThenCallsChatRepository()
        {
            // Act
            this.apiService.SubmitFeedback(1, 5, "Great experience");

            // Assert
            this.chatRepositoryMock.Verify(repository =>
                repository.SaveSessionRatingAndFeedback(1, 5, "Great experience"), Times.Once);
        }

        [Fact]
        public async Task GetAmortizationAsync_WhenCalled_ThenCallsLoanServiceAndReturnsAmortizationRows()
        {
            // Arrange
            var expectedAmortizationRows = new List<AmortizationRow> { new AmortizationRow { InstallmentNumber = 1 } };
            this.loanServiceMock.Setup(service => service.GetAmortizationAsync(1))
                .ReturnsAsync(expectedAmortizationRows);

            // Act
            var actualAmortizationResult = await this.apiService.GetAmortizationAsync(1);

            // Assert
            Assert.Equal(expectedAmortizationRows, actualAmortizationResult);
            this.loanServiceMock.Verify(service => service.GetAmortizationAsync(1), Times.Once);
        }
    }
}