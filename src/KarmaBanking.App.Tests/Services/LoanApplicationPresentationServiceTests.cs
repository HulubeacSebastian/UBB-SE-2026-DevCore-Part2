// <copyright file="LoanApplicationPresentationServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using KarmaBanking.App.Services;
    using Xunit;

    public class LoanApplicationPresentationServiceTests
    {
        [Fact]
        public void BuildApplicationOutcome_WhenRejectionReasonIsNull_ThenReturnsApproved()
        {
            // Arrange
            var loanApplicationPresentationService = new LoanApplicationPresentationService();

            // Act
            var (isApprovedResult, applicationOutcomeMessage) = loanApplicationPresentationService.BuildApplicationOutcome(null);

            // Assert
            Assert.True(isApprovedResult);
            Assert.Equal("Your loan application has been approved!", applicationOutcomeMessage);
        }

        [Fact]
        public void BuildApplicationOutcome_WhenRejectionReasonProvided_ThenReturnsRejectedWithMessage()
        {
            // Arrange
            var loanApplicationPresentationService = new LoanApplicationPresentationService();
            string rejectionReasonText = "Credit score too low";

            // Act
            var (isApprovedResult, applicationOutcomeMessage) = loanApplicationPresentationService.BuildApplicationOutcome(rejectionReasonText);

            // Assert
            Assert.False(isApprovedResult);
            Assert.Equal($"Application rejected: {rejectionReasonText}", applicationOutcomeMessage);
        }
    }
}