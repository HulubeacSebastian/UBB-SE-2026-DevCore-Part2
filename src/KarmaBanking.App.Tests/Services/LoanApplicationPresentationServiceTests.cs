namespace KarmaBanking.App.Tests.Services
{
    using KarmaBanking.App.Services;
    using Xunit;

    public class LoanApplicationPresentationServiceTests
    {
        [Fact]
        public void BuildApplicationOutcome_NullRejectionReason_ReturnsApproved()
        {
            var loanApplicationPresentationService = new LoanApplicationPresentationService();

            var (isApproved, returnedMessage) = loanApplicationPresentationService.BuildApplicationOutcome(null);

            Assert.True(isApproved);
            Assert.Equal("Your loan application has been approved!", returnedMessage);
        }

        [Fact]
        public void BuildApplicationOutcome_WithRejectionReason_ReturnsRejectedWithMessage()
        {
            var loanApplicationPresentationService = new LoanApplicationPresentationService();
            var reasonForRejection = "Credit score too low";

            var (isApproved, returnedMessage) = loanApplicationPresentationService.BuildApplicationOutcome(reasonForRejection);

            Assert.False(isApproved);
            Assert.Equal($"Application rejected: {reasonForRejection}", returnedMessage);
        }
    }
}