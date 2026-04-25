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

    public class ApiServiceChatTests
    {
        private readonly Mock<IChatRepository> mockChatRepository;
        private readonly Mock<ILoanService> mockLoanService;
        private readonly ApiService apiService;

        public ApiServiceChatTests()
        {
            this.mockChatRepository = new Mock<IChatRepository>();
            this.mockLoanService = new Mock<ILoanService>();

            this.apiService = new ApiService(this.mockLoanService.Object, this.mockChatRepository.Object);
        }

        [Fact]
        public async Task CreateChatSessionAsync_CallsRepositoryAndReturnsId()
        {
            int userId = 1;
            string category = "Technical Support";
            int expectedSessionId = 100;
            this.mockChatRepository.Setup(chatRepo => chatRepo.CreateChatSessionAsync(userId, category))
                .ReturnsAsync(expectedSessionId);

            int createdSessionId = await this.apiService.CreateChatSessionAsync(userId, category);

            Assert.Equal(expectedSessionId, createdSessionId);
            this.mockChatRepository.Verify(chatRepo => chatRepo.CreateChatSessionAsync(userId, category), Times.Once);
        }

        [Fact]
        public async Task SendMessageAsync_CallsRepositoryWithCorrectData()
        {
            var newMessage = new ChatMessage
            {
                SessionId = 100,
                Content = "Test message",
                SenderType = "User",
                SentAt = DateTime.Now
            };

            await this.apiService.SendMessageAsync(newMessage);

            var expectedSessionIdOfMessage = 100;
            var expectedContent = "Test message";
            var expectedMessageSenderType = "User";

            this.mockChatRepository.Verify(chatRepo => chatRepo.AddChatMessageAsync(It.Is<ChatMessage>(message =>
                message.SessionId == expectedSessionIdOfMessage &&
                message.Content == expectedContent &&
                message.SenderType == expectedMessageSenderType)), Times.Once);
        }

        [Fact]
        public async Task GetUserChatSessionsAsync_ReturnsListFromRepository()
        {
            var expectedSessions = new List<ChatSession>
            {
                new ChatSession { Id = 1, IssueCategory = "Billing" },
                new ChatSession { Id = 2, IssueCategory = "Security" }
            };
            this.mockChatRepository.Setup(chatRepo => chatRepo.GetChatSessionsAsync())
                .ReturnsAsync(expectedSessions);

            var chatSessionList = await this.apiService.GetUserChatSessionsAsync();

            Assert.Equal(2, chatSessionList.Count);
            Assert.Equal("Billing", chatSessionList[0].IssueCategory);
            this.mockChatRepository.Verify(chatRepo => chatRepo.GetChatSessionsAsync(), Times.Once);
        }

        [Fact]
        public void SubmitFeedback_CallsRepositoryWithCorrectParameters()
        {
            int sessionId = 100;
            int rating = 5;
            string feedback = "Great service!";

            this.apiService.SubmitFeedback(sessionId, rating, feedback);

            this.mockChatRepository.Verify(chatRepo => chatRepo.SaveSessionRatingAndFeedback(
                sessionId, rating, feedback), Times.Once);
        }

        [Fact]
        public async Task GetChatbotPresetQuestionsAsync_ReturnsKeysFromDefaultDictionary()
        {
            var chatbotPresetQuestionList = await this.apiService.GetChatbotPresetQuestionsAsync();

            Assert.Contains("How do I reset my password?", chatbotPresetQuestionList);
            Assert.Contains("Why was my card declined?", chatbotPresetQuestionList);
            Assert.True(chatbotPresetQuestionList.Count >= 5);
        }

        [Fact]
        public async Task GetChatbotPresetAnswerAsync_ReturnsCorrectAnswerForKnownQuestion()
        {
            string question = "How long does a transfer take?";

            var answer = await this.apiService.GetChatbotPresetAnswerAsync(question);

            Assert.Contains("Internal transfers are usually immediate", answer);
        }

        [Fact]
        public async Task GetChatbotPresetAnswerAsync_ReturnsDefaultMessageForUnknownQuestion()
        {
            string question = "Test message";

            var answer = await this.apiService.GetChatbotPresetAnswerAsync(question);

            Assert.Equal("Please contact the team for more help with this topic.", answer);
        }

        [Fact]
        public async Task SendChatToSupportAsync_ReturnsTrue_WhenInputIsValid()
        {
            var chatSentSuccesfully = await this.apiService.SendChatToSupportAsync("Transcript text", "Help me", null);

            Assert.True(chatSentSuccesfully);
        }

        [Fact]
        public async Task SendChatToSupportAsync_ReturnsFalse_WhenInputsAreEmpty()
        {
            var chatSentSuccesfully = await this.apiService.SendChatToSupportAsync(string.Empty, " ", null);

            Assert.False(chatSentSuccesfully);
        }
    }
}