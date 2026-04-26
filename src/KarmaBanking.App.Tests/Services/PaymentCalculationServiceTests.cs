// <copyright file="PaymentCalculationServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using System.Globalization;
    using KarmaBanking.App.Services;
    using Xunit;

    public class PaymentCalculationServiceTests
    {
        private readonly PaymentCalculationService paymentCalculationService;

        public PaymentCalculationServiceTests()
        {
            this.paymentCalculationService = new PaymentCalculationService();
        }

        [Theory]
        [InlineData(100, 1000, 10, true, 0, 900, 9)]
        [InlineData(100, 1000, 10, false, 200, 800, 8)]
        public void CalculatePaymentPreview_ReturnsExpectedValues(
            decimal monthlyInstallmentAmount,
            decimal currentOutstandingBalance,
            int remainingMonthsCount,
            bool isStandardPaymentSelected,
            decimal customPaymentAmountValue,
            decimal expectedBalanceAfterPayment,
            int expectedRemainingMonthsAfterPayment)
        {
            // Act
            var calculationResult = this.paymentCalculationService.CalculatePaymentPreview(
                monthlyInstallmentAmount,
                currentOutstandingBalance,
                remainingMonthsCount,
                isStandardPaymentSelected,
                customPaymentAmountValue);

            // Assert
            Assert.Equal(expectedBalanceAfterPayment, calculationResult.BalanceAfterPayment);
            Assert.Equal(expectedRemainingMonthsAfterPayment, calculationResult.RemainingMonths);
        }
    }
}