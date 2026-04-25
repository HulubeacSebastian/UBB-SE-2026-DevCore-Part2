using System.Globalization;
using KarmaBanking.App.Services;
using Xunit;

namespace KarmaBanking.App.Tests.Services;

public class PaymentCalculationServiceTests
{
    private readonly PaymentCalculationService paymentCalculator;

    public PaymentCalculationServiceTests()
    {
        this.paymentCalculator = new PaymentCalculationService();
    }

    [Theory]
    [InlineData(100, 1000, 10, true, 0, 900, 9)]
    [InlineData(100, 1000, 10, false, 200, 800, 8)]
    [InlineData(100, 1000, 10, false, 0, 1000, 10)]
    [InlineData(100, 1000, 10, false, -50, 1050, 10)]
    [InlineData(100, 500, 10, false, 600, 0, 4)]
    [InlineData(100, 500, 2, false, 300, 200, 0)]
    public void CalculatePaymentPreview_ReturnsExpectedTuple(
        decimal monthlyInstallment,
        decimal outstandingBalance,
        int remainingMonths,
        bool isStandardPayment,
        decimal customPaymentAmount,
        decimal expectedBalance,
        int expectedRemainingMonthsAfterPayment)
    {
        var (balanceAfterPayment, remainingMonthsAfterPayment) = this.paymentCalculator.CalculatePaymentPreview(
            monthlyInstallment, outstandingBalance, remainingMonths, isStandardPayment, customPaymentAmount);

        Assert.Equal(expectedBalance, balanceAfterPayment);
        Assert.Equal(expectedRemainingMonthsAfterPayment, remainingMonthsAfterPayment);
    }

    [Theory]
    [InlineData(null, false, 0)]
    [InlineData("", false, 0)]
    [InlineData("   ", false, 0)]
    [InlineData("invalid_input", false, 0)]
    public void ParsePaymentAmount_InvalidOrEmptyInput_ReturnsFalse(string input, bool expectedSuccess, decimal expectedAmount)
    {
        var (succesfullyParsed, parsedPaymentString) = this.paymentCalculator.ParsePaymentAmount(input);

        Assert.Equal(expectedSuccess, succesfullyParsed);
        Assert.Equal(expectedAmount, parsedPaymentString);
    }

    [Fact]
    public void ParsePaymentAmount_ValidInput_ReturnsParsedAmount()
    {
        string input = 1234.56m.ToString(CultureInfo.CurrentCulture);

        var (succesfullyParsed, parsedPaymentString) = this.paymentCalculator.ParsePaymentAmount(input);

        Assert.True(succesfullyParsed);
        Assert.Equal(1234.56m, parsedPaymentString);
    }

    [Fact]
    public void ParsePaymentAmount_InvariantInput_ReturnsParsedAmount()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
            string input = "1234.56";

            var (succesfullyParsed, parsedPaymentString) = this.paymentCalculator.ParsePaymentAmount(input);

            Assert.True(succesfullyParsed);
            Assert.Equal(1234.56m, parsedPaymentString);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData(0, 1000, false, "Payment amount must be greater than 0.")]
    [InlineData(-50, 1000, false, "Payment amount must be greater than 0.")]
    public void ValidatePaymentAmount_LessOrEqualZero_ReturnsFalse(decimal payment, decimal balance, bool expectedValid, string expectedMsg)
    {
        var (isValid, validationMessage) = this.paymentCalculator.ValidatePaymentAmount(payment, balance);
        Assert.Equal(expectedValid, isValid);
        Assert.Equal(expectedMsg, validationMessage);
    }

    [Fact]
    public void ValidatePaymentAmount_ExceedsBalance_ReturnsFalse()
    {
        decimal payment = 1500m;
        decimal balance = 1000m;
        string expectedMessage = $"Payment amount cannot exceed outstanding balance of {balance:C2}.";

        var (isValid, validationMessage) = this.paymentCalculator.ValidatePaymentAmount(payment, balance);

        Assert.False(isValid);
        Assert.Equal(expectedMessage, validationMessage);
    }

    [Fact]
    public void ValidatePaymentAmount_ValidAmount_ReturnsTrue()
    {
        var (isValid, validationMessage) = this.paymentCalculator.ValidatePaymentAmount(500m, 1000m);

        Assert.True(isValid);
        Assert.Empty(validationMessage);
    }

    [Theory]
    [InlineData(100.0, 500.0, 600.0, 500.0)]
    [InlineData(100.0, 500.0, 250.0, 250.0)]
    [InlineData(600.0, 500.0, null, 500.0)]
    [InlineData(100.0, 500.0, null, 100.0)]
    public void GetInitialCustomAmount_ReturnsExpectedValue(
        double monthlyInstallment,
        double outstandingBalance,
        double? currentCustomAmount,
        double expectedAmount)
    {
        var initialCustomAmount = paymentCalculator.GetInitialCustomAmount(
            (decimal)monthlyInstallment,
            (decimal)outstandingBalance,
            currentCustomAmount);

        Assert.Equal((decimal)expectedAmount, initialCustomAmount);
    }

    [Fact]
    public void FormatCustomAmount_FormatsAccordingToCulture()
    {
        decimal amount = 1234.5678m;
        string expected = amount.ToString("0.##", CultureInfo.CurrentCulture);

        var formattedCustomAmount = this.paymentCalculator.FormatCustomAmount(amount);

        Assert.Equal(expected, formattedCustomAmount);
    }
}