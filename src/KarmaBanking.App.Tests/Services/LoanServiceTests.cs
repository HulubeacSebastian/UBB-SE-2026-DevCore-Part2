// <copyright file="LoanServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using KarmaBanking.App.Models;
    using KarmaBanking.App.Models.Enums;
    using KarmaBanking.App.Repositories.Interfaces;
    using KarmaBanking.App.Services;
    using Moq;
    using Xunit;

    public class LoanServiceTests
    {
        private readonly Mock<ILoanRepository> loanRepositoryMock;
        private readonly LoanService loanService;

        public LoanServiceTests()
        {
            this.loanRepositoryMock = new Mock<ILoanRepository>();
            this.loanService = new LoanService(this.loanRepositoryMock.Object);
        }

        [Fact]
        public async Task ProcessApplicationStatusAsync_WhenUserHasFiveActiveLoans_RejectsApplication()
        {
            // Arrange
            this.loanRepositoryMock.Setup(repository => repository.GetLoansByUserAsync(1)).ReturnsAsync(new List<Loan>
            {
                new() { LoanStatus = LoanStatus.Active, OutstandingBalance = 1000m },
                new() { LoanStatus = LoanStatus.Active, OutstandingBalance = 1000m },
                new() { LoanStatus = LoanStatus.Active, OutstandingBalance = 1000m },
                new() { LoanStatus = LoanStatus.Active, OutstandingBalance = 1000m },
                new() { LoanStatus = LoanStatus.Active, OutstandingBalance = 1000m },
            });

            var loanApplicationInstance = new LoanApplication
            {
                IdentificationNumber = 10,
                UserIdentificationNumber = 1,
                LoanType = LoanType.Personal,
                DesiredAmount = 1000m,
                PreferredTermMonths = 12,
                Purpose = "Home office",
            };

            // Act
            var (applicationStatus, rejectionReason) = await this.loanService.ProcessApplicationStatusAsync(loanApplicationInstance);

            // Assert
            Assert.Equal(LoanApplicationStatus.Rejected, applicationStatus);
            Assert.Equal("Maximum number of active loans reached.", rejectionReason);
            this.loanRepositoryMock.Verify(repository => repository.UpdateLoanApplicationStatusAsync(
                10,
                LoanApplicationStatus.Rejected,
                "Maximum number of active loans reached."), Times.Once);
        }

        [Fact]
        public async Task ProcessApplicationStatusAsync_WhenDebtLimitExceeded_RejectsApplication()
        {
            // Arrange
            this.loanRepositoryMock.Setup(repository => repository.GetLoansByUserAsync(1)).ReturnsAsync(new List<Loan>
            {
                new() { LoanStatus = LoanStatus.Active, OutstandingBalance = 190000m },
            });

            var loanApplicationInstance = new LoanApplication
            {
                IdentificationNumber = 11,
                UserIdentificationNumber = 1,
                LoanType = LoanType.Personal,
                DesiredAmount = 10000m,
                PreferredTermMonths = 24,
                Purpose = "Consolidation",
            };

            // Act
            var (applicationStatus, rejectionReason) = await this.loanService.ProcessApplicationStatusAsync(loanApplicationInstance);

            // Assert
            Assert.Equal(LoanApplicationStatus.Rejected, applicationStatus);
            Assert.Equal("Total debt limit exceeded.", rejectionReason);
            this.loanRepositoryMock.Verify(repository => repository.UpdateLoanApplicationStatusAsync(
                11,
                LoanApplicationStatus.Rejected,
                "Total debt limit exceeded."), Times.Once);
        }

        [Fact]
        public async Task ProcessApplicationStatusAsync_WhenRulesPass_ApprovesApplication()
        {
            // Arrange
            this.loanRepositoryMock.Setup(repository => repository.GetLoansByUserAsync(1)).ReturnsAsync(new List<Loan>
            {
                new() { LoanStatus = LoanStatus.Active, OutstandingBalance = 5000m },
            });

            var loanApplicationInstance = new LoanApplication
            {
                IdentificationNumber = 12,
                UserIdentificationNumber = 1,
                LoanType = LoanType.Auto,
                DesiredAmount = 10000m,
                PreferredTermMonths = 36,
                Purpose = "Car",
            };

            // Act
            var (applicationStatus, rejectionReason) = await this.loanService.ProcessApplicationStatusAsync(loanApplicationInstance);

            // Assert
            Assert.Equal(LoanApplicationStatus.Approved, applicationStatus);
            Assert.Null(rejectionReason);
            this.loanRepositoryMock.Verify(repository => repository.UpdateLoanApplicationStatusAsync(12, LoanApplicationStatus.Approved, null), Times.Once);
        }

        [Fact]
        public async Task PayInstallmentAsync_StandardPayment_UpdatesBalanceAndRemainingMonths()
        {
            // Arrange
            this.loanRepositoryMock.Setup(repository => repository.GetLoanByIdAsync(20)).ReturnsAsync(new Loan
            {
                Id = 20,
                OutstandingBalance = 1000m,
                MonthlyInstallment = 200m,
                RemainingMonths = 5,
                LoanStatus = LoanStatus.Active,
            });

            // Act
            await this.loanService.PayInstallmentAsync(20, null);

            // Assert
            this.loanRepositoryMock.Verify(repository => repository.UpdateLoanAfterPaymentAsync(20, 800m, 4, LoanStatus.Active), Times.Once);
        }

        [Fact]
        public async Task PayInstallmentAsync_CustomPaymentBelowInstallment_Throws()
        {
            // Arrange
            this.loanRepositoryMock.Setup(repository => repository.GetLoanByIdAsync(21)).ReturnsAsync(new Loan
            {
                Id = 21,
                OutstandingBalance = 1000m,
                MonthlyInstallment = 200m,
                RemainingMonths = 5,
                LoanStatus = LoanStatus.Active,
            });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => this.loanService.PayInstallmentAsync(21, 150m));
            this.loanRepositoryMock.Verify(repository => repository.UpdateLoanAfterPaymentAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<LoanStatus>()), Times.Never);
        }

        [Fact]
        public async Task PayInstallmentAsync_WhenLoanGetsPaidOff_ClosesLoan()
        {
            // Arrange
            this.loanRepositoryMock.Setup(repository => repository.GetLoanByIdAsync(22)).ReturnsAsync(new Loan
            {
                Id = 22,
                OutstandingBalance = 600m,
                MonthlyInstallment = 200m,
                RemainingMonths = 3,
                LoanStatus = LoanStatus.Active,
            });

            // Act
            await this.loanService.PayInstallmentAsync(22, 600m);

            // Assert
            this.loanRepositoryMock.Verify(repository => repository.UpdateLoanAfterPaymentAsync(22, 0m, 0, LoanStatus.Passed), Times.Once);
        }

        [Fact]
        public void CalculatePaymentPreview_WithCustomAmount_ComputesPreviewValues()
        {
            // Arrange
            var loanInstance = new Loan
            {
                MonthlyInstallment = 250m,
                OutstandingBalance = 1000m,
                RemainingMonths = 6,
            };

            // Act
            var (balanceAfterPayment, remainingMonthsCount) = this.loanService.CalculatePaymentPreview(loanInstance, 500m);

            // Assert
            Assert.Equal(500m, balanceAfterPayment);
            Assert.Equal(4, remainingMonthsCount);
        }

        [Fact]
        public void ParseCustomPaymentAmount_InvalidInput_ReturnsNull()
        {
            // Act
            var parsedAmountValue = this.loanService.ParseCustomPaymentAmount("not-a-number");

            // Assert
            Assert.Null(parsedAmountValue);
        }

        [Fact]
        public async Task SubmitLoanApplicationAsync_WhenApproved_CreatesLoanAndAmortization()
        {
            // Arrange
            this.loanRepositoryMock.Setup(repository => repository.CreateLoanApplicationAsync(It.IsAny<LoanApplicationRequest>())).ReturnsAsync(30);
            this.loanRepositoryMock.Setup(repository => repository.GetLoansByUserAsync(1)).ReturnsAsync(new List<Loan>());
            this.loanRepositoryMock.Setup(repository => repository.CreateLoanAsync(It.IsAny<Loan>())).ReturnsAsync(40);
            this.loanRepositoryMock.Setup(repository => repository.GetLoanByIdAsync(40)).ReturnsAsync(new Loan
            {
                Id = 40,
                UserId = 1,
                LoanType = LoanType.Personal,
                Principal = 12000m,
                OutstandingBalance = 12000m,
                InterestRate = 8.5m,
                MonthlyInstallment = 376.92m,
                RemainingMonths = 36,
                LoanStatus = LoanStatus.Active,
                TermInMonths = 36,
                StartDate = DateTime.Today,
            });

            var loanApplicationRequestInstance = new LoanApplicationRequest
            {
                UserId = 1,
                LoanType = LoanType.Personal,
                DesiredAmount = 12000m,
                PreferredTermMonths = 36,
                Purpose = "Renovation",
            };

            // Act
            var (applicationStatus, rejectionReason) = await this.loanService.SubmitLoanApplicationAsync(loanApplicationRequestInstance);

            // Assert
            Assert.Equal(LoanApplicationStatus.Approved, applicationStatus);
            Assert.Null(rejectionReason);
            this.loanRepositoryMock.Verify(repository => repository.UpdateLoanApplicationStatusAsync(30, LoanApplicationStatus.Approved, null), Times.Once);
            this.loanRepositoryMock.Verify(repository => repository.CreateLoanAsync(It.IsAny<Loan>()), Times.Once);
            this.loanRepositoryMock.Verify(repository => repository.SaveAmortizationAsync(It.Is<List<AmortizationRow>>(rows => rows.Count == 36)), Times.Once);
        }

        [Fact]
        public async Task SubmitLoanApplicationAsync_WhenRejected_DoesNotCreateLoanOrAmortization()
        {
            // Arrange
            this.loanRepositoryMock.Setup(repository => repository.CreateLoanApplicationAsync(It.IsAny<LoanApplicationRequest>())).ReturnsAsync(31);
            this.loanRepositoryMock.Setup(repository => repository.GetLoansByUserAsync(1)).ReturnsAsync(new List<Loan>
            {
                new() { LoanStatus = LoanStatus.Active, OutstandingBalance = 199500m },
            });

            var loanApplicationRequestInstance = new LoanApplicationRequest
            {
                UserId = 1,
                LoanType = LoanType.Personal,
                DesiredAmount = 1000m,
                PreferredTermMonths = 12,
                Purpose = "Emergency",
            };

            // Act
            var (applicationStatus, rejectionReason) = await this.loanService.SubmitLoanApplicationAsync(loanApplicationRequestInstance);

            // Assert
            Assert.Equal(LoanApplicationStatus.Rejected, applicationStatus);
            Assert.Equal("Total debt limit exceeded.", rejectionReason);
            this.loanRepositoryMock.Verify(repository => repository.UpdateLoanApplicationStatusAsync(31, LoanApplicationStatus.Rejected, "Total debt limit exceeded."), Times.Once);
            this.loanRepositoryMock.Verify(repository => repository.CreateLoanAsync(It.IsAny<Loan>()), Times.Never);
            this.loanRepositoryMock.Verify(repository => repository.SaveAmortizationAsync(It.IsAny<List<AmortizationRow>>()), Times.Never);
        }

        [Fact]
        public async Task PayInstallmentAsync_WhenPaymentExceedsOutstanding_Throws()
        {
            // Arrange
            this.loanRepositoryMock.Setup(repository => repository.GetLoanByIdAsync(23)).ReturnsAsync(new Loan
            {
                Id = 23,
                OutstandingBalance = 500m,
                MonthlyInstallment = 100m,
                RemainingMonths = 5,
                LoanStatus = LoanStatus.Active,
            });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => this.loanService.PayInstallmentAsync(23, 600m));
            this.loanRepositoryMock.Verify(repository => repository.UpdateLoanAfterPaymentAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<LoanStatus>()), Times.Never);
        }

        [Fact]
        public async Task PayInstallmentAsync_WhenLoanAlreadyClosed_Throws()
        {
            // Arrange
            this.loanRepositoryMock.Setup(repository => repository.GetLoanByIdAsync(24)).ReturnsAsync(new Loan
            {
                Id = 24,
                OutstandingBalance = 0m,
                MonthlyInstallment = 100m,
                RemainingMonths = 0,
                LoanStatus = LoanStatus.Passed,
            });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => this.loanService.PayInstallmentAsync(24, null));
        }

        [Fact]
        public void NormalizeCustomPaymentAmount_WhenOverBalance_CapsToOutstanding()
        {
            // Arrange
            var loanInstance = new Loan
            {
                MonthlyInstallment = 200m,
                OutstandingBalance = 150m,
                RemainingMonths = 1,
            };

            // Act
            decimal normalizedAmountValue = this.loanService.NormalizeCustomPaymentAmount(loanInstance, 300m);

            // Assert
            Assert.Equal(150m, normalizedAmountValue);
        }

        [Fact]
        public void GetRepaymentProgress_WhenPrincipalIsZero_ReturnsZero()
        {
            // Arrange
            var loanInstance = new Loan
            {
                Principal = 0m,
                OutstandingBalance = 0m,
            };

            // Act
            double progressResult = this.loanService.GetRepaymentProgress(loanInstance);

            // Assert
            Assert.Equal(0d, progressResult);
        }
    }
}