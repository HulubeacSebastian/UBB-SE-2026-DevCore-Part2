namespace KarmaBanking.App.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using KarmaBanking.App.Models;
    using KarmaBanking.App.Models.DTOs;
    using KarmaBanking.App.Models.Enums;
    using KarmaBanking.App.Repositories.Interfaces;
    using KarmaBanking.App.Services;
    using KarmaBanking.App.Services.Interfaces;
    using NSubstitute;
    using Xunit;

    public class ApiServiceTests
    {
        [Fact]
        public async Task GetChatbotPresetQuestionsAsync_ReturnsExpectedQuestions()
        {
            var chatbotQuestionGetter = new ApiService();

            var chatbotQuestionList = await chatbotQuestionGetter.GetChatbotPresetQuestionsAsync();

            Assert.Contains("How do I reset my password?", chatbotQuestionList);
            Assert.Contains("Why was my card declined?", chatbotQuestionList);
            Assert.Contains("How long does a transfer take?", chatbotQuestionList);
            Assert.True(chatbotQuestionList.Count >= 5);
        }

        [Theory]
        [InlineData("How do I reset my password?", "You can reset your password from the login screen by choosing Forgot password and following the verification steps.")]
        [InlineData("Why was my card declined?", "A card can be declined because of insufficient funds, an expired card, a blocked card, or a merchant validation issue. Please check the card status in the app first.")]
        public async Task GetChatbotPresetAnswerAsync_KnownQuestion_ReturnsExpectedAnswer(string question, string expectedAnswer)
        {
            var chatbotQuestionGetter = new ApiService();

            var chatbotProvidedAnswear = await chatbotQuestionGetter.GetChatbotPresetAnswerAsync(question);

            Assert.Equal(expectedAnswer, chatbotProvidedAnswear);
        }

        [Fact]
        public async Task GetChatbotPresetAnswerAsync_UnknownQuestion_ReturnsFallbackMessage()
        {
            var chatbotQuestionGetter = new ApiService();

            var chatbotProvidedAnswear = await chatbotQuestionGetter.GetChatbotPresetAnswerAsync("What is the meaning of life?");

            Assert.Equal("Please contact the team for more help with this topic.", chatbotProvidedAnswear);
        }

        [Fact]
        public async Task SendChatToSupportAsync_WithContent_ReturnsTrue()
        {
            var supportChatManager = new ApiService();
            var testAttachment = new SelectedAttachment { FileName = "test.png" };

            var chatSentSuccesfully = await supportChatManager.SendChatToSupportAsync("Transcript data", "User message", testAttachment);

            Assert.True(chatSentSuccesfully);
        }

        [Fact]
        public async Task SendChatToSupportAsync_NoContent_ReturnsFalse()
        {
            var supportChatManager = new ApiService();

            var chatSentSuccesfully = await supportChatManager.SendChatToSupportAsync(string.Empty, string.Empty, null);

            Assert.False(chatSentSuccesfully);
        }

        [Fact]
        public async Task GetAllLoansAsync_CallsLoanService()
        {
            var mockLoanService = Substitute.For<ILoanService>();
            var mockChatRepo = Substitute.For<IChatRepository>();
            var expectedLoans = new List<Loan> { new Loan { IdentificationNumber = 1 } };
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

            var chatFeedbackSubmitter = new ApiService(mockLoanService, mockChatRepo);
            chatFeedbackSubmitter.SubmitFeedback(1, 5, "Great service");

            mockChatRepo.Received(1).SaveSessionRatingAndFeedback(1, 5, "Great service");
        }

        [Fact]
        public async Task GetAmortizationAsync_CallsLoanService()
        {
            var mockLoanService = Substitute.For<ILoanService>();
            var mockChatRepo = Substitute.For<IChatRepository>();
            var expectedRowsForAmortization = new List<AmortizationRow> { new AmortizationRow { InstallmentNumber = 1 } };
            mockLoanService.GetAmortizationAsync(1).Returns(Task.FromResult(expectedRows));

            var amortizationGetter = new ApiService(mockLoanService, mockChatRepo);
            var amortizationRowList = await amortizationGetter.GetAmortizationAsync(1);

            Assert.Equal(expectedRowsForAmortization, amortizationRowList);
            await mockLoanService.Received(1).GetAmortizationAsync(1);
        }
    }
}