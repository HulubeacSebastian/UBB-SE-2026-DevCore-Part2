// <copyright file="ApiServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using KarmaBanking.App.Models;
    using KarmaBanking.App.Models.Enums;
    using KarmaBanking.App.Repositories.Interfaces;
    using KarmaBanking.App.Services;
    using KarmaBanking.App.Services.Interfaces;
    using Moq;
    using Xunit;

    public class ApiServiceTests
    {
        private readonly Mock<ILoanService> mockLoanService;
        private readonly Mock<IChatRepository> mockChatRepository;
        private readonly ApiService apiService;

        public ApiServiceTests()
        {
            this.mockLoanService = new Mock<ILoanService>();
            this.mockChatRepository = new Mock<IChatRepository>();
            this.apiService = new ApiService(this.mockLoanService.Object, this.mockChatRepository.Object);
        }

        [Fact]
        public async Task GetAllLoansAsync_CallsLoanService()
        {
            var mockLoanService = Substitute.For<ILoanService>();
            var mockChatRepo = Substitute.For<IChatRepository>();
            var expectedLoans = new List<Loan> { new Loan { Id = 1 } };
            mockLoanService.GetAllLoansAsync().Returns(Task.FromResult(expectedLoans));

            var loansGetter = new ApiService(mockLoanService, mockChatRepo);
            var loanListInRepo = await loansGetter.GetAllLoansAsync();

            Assert.Equal(expectedLoans, loanListInRepo);
            await mockLoanService.Received(1).GetAllLoansAsync();
        }

        [Fact]
        public async Task ApplyForLoanAsync_CallsLoanServiceAndReturnsRejectionReason()
        {
            var mockLoanService = Substitute.For<ILoanService>();
            var mockChatRepo = Substitute.For<IChatRepository>();

            var applicationRequest = new LoanApplicationRequest
            {
                DesiredAmount = 5000,
                Purpose = "Home Renovation"
            };

            mockLoanService.SubmitLoanApplicationAsync(applicationRequest)
                .Returns(Task.FromResult((LoanApplicationStatus.Rejected, "Credit score too low")));

            var loanApplier = new ApiService(mockLoanService, mockChatRepo);
            var reveivedMessageForAppliedLoan = await loanApplier.ApplyForLoanAsync(applicationRequest);

            Assert.Equal("Credit score too low", reveivedMessageForAppliedLoan);
            await mockLoanService.Received(1).SubmitLoanApplicationAsync(applicationRequest);
        }

        [Fact]
        public async Task CreateChatSessionAsync_CallsChatRepository()
        {
            var mockLoanService = Substitute.For<ILoanService>();
            var mockChatRepo = Substitute.For<IChatRepository>();
            mockChatRepo.CreateChatSessionAsync(1, "Account").Returns(Task.FromResult(99));

            var chatSessionCreator = new ApiService(mockLoanService, mockChatRepo);
            var userIdForCreatedSession = await chatSessionCreator.CreateChatSessionAsync(1, "Account");

            Assert.Equal(99, userIdForCreatedSession);
            await mockChatRepo.Received(1).CreateChatSessionAsync(1, "Account");
        }

        [Fact]
        public void SubmitFeedback_CallsChatRepository()
        {
            var mockLoanService = Substitute.For<ILoanService>();
            var mockChatRepo = Substitute.For<IChatRepository>();

            var result = await this.apiService.GetAllLoansAsync();

            Assert.Equal(expectedLoans, result);
            this.mockLoanService.Verify(s => s.GetAllLoansAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAmortizationAsync_CallsLoanService()
        {
            var mockLoanService = Substitute.For<ILoanService>();
            var mockChatRepo = Substitute.For<IChatRepository>();
            var expectedRowsForAmortization = new List<AmortizationRow> { new AmortizationRow { InstallmentNumber = 1 } };
            // mockLoanService.GetAmortizationAsync(1).Returns(Task.FromResult(expectedRows));
            var amortizationGetter = new ApiService(mockLoanService, mockChatRepo);
            var amortizationRowList = await amortizationGetter.GetAmortizationAsync(1);

            Assert.Equal(expectedRows, result);
            this.mockLoanService.Verify(s => s.GetAmortizationAsync(1), Times.Once);
        }
    }
}