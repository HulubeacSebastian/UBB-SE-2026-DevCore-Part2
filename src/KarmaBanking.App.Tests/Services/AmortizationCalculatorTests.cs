// <copyright file="AmortizationCalculatorTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using System;
    using System.Linq;
    using global::KarmaBanking.App.Utils;
    using Xunit;

    public class AmortizationCalculatorTests
    {
        [Fact]
        public void ComputeEstimate_WhenZeroInterest_ThenSplitsPrincipalEvenly()
        {
            // Act
            var loanEstimate = AmortizationCalculator.ComputeEstimate(1200m, 0m, 12);

            // Assert
            Assert.Equal(0m, loanEstimate.IndicativeRate);
            Assert.Equal(100m, loanEstimate.MonthlyInstallment);
            Assert.Equal(1200m, loanEstimate.TotalRepayable);
        }

        [Fact]
        public void ComputeRepaymentProgress_WhenHalfBalancePaid_ThenReturnsFiftyPercent()
        {
            // Act
            decimal repaymentProgress = AmortizationCalculator.ComputeRepaymentProgress(1000m, 500m);

            // Assert
            Assert.Equal(50m, repaymentProgress);
        }

        [Fact]
        public void Generate_WhenCalculatingAmortization_ThenBuildsExpectedNumberOfRowsAndEndsAtZeroBalance()
        {
            // Arrange
            var loanInstance = new Loan
            {
                Id = 7,
                Principal = 1200m,
                OutstandingBalance = 1200m,
                InterestRate = 0m,
                MonthlyInstallment = 100m,
                RemainingMonths = 12,
                TermInMonths = 12,
                StartDate = new DateTime(2026, 1, 1),
            };

            // Act
            var amortizationRows = AmortizationCalculator.Generate(loanInstance);

            // Assert
            Assert.Equal(12, amortizationRows.Count);
            Assert.Equal(0m, amortizationRows[^1].RemainingBalance);
        }

        [Fact]
        public void Generate_WhenLoanStartedMonthsAgo_ThenMarksOnlyOneCurrentInstallment()
        {
            // Arrange
            var loanInstance = new Loan
            {
                Id = 8,
                Principal = 10000m,
                OutstandingBalance = 10000m,
                InterestRate = 8m,
                MonthlyInstallment = 313.36m,
                RemainingMonths = 36,
                TermInMonths = 36,
                StartDate = DateTime.Today.AddMonths(-2),
            };

            // Act
            var amortizationRows = AmortizationCalculator.Generate(loanInstance);
            int currentInstallmentCount = amortizationRows.Count(row => row.IsCurrent);

            // Assert
            Assert.Equal(1, currentInstallmentCount);
        }
    }
}