// <copyright file="ChatServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using System.Threading.Tasks;
    using global::KarmaBanking.App.Repositories.Interfaces;
    using global::KarmaBanking.App.Services;
    using Moq;
    using Xunit;

    public class ApiServiceChatTests
    {
        private readonly Mock<IChatRepository> chatRepositoryMock;
        private readonly Mock<ILoanService> loanServiceMock;
        private readonly ApiService apiService;

        public ApiServiceChatTests()
        {
            this.chatRepositoryMock = new Mock<IChatRepository>();
            this.loanServiceMock = new Mock<ILoanService>();
            this.apiService = new ApiService(this.loanServiceMock.Object, this.chatRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateChatSessionAsync_WhenCalled_ThenCallsRepositoryAndReturnsSessionId()
        {
            // Arrange
            int userIdentificationNumber = 1;
            string issueCategoryName = "Support";
            int expectedSessionIdentificationNumber = 100;

            this.chatRepositoryMock.Setup(repository => repository.CreateChatSessionAsync(userIdentificationNumber, issueCategoryName))
                .ReturnsAsync(expectedSessionIdentificationNumber);

            // Act
            int actualSessionIdentificationNumber = await this.apiService.CreateChatSessionAsync(userIdentificationNumber, issueCategoryName);

            // Assert
            Assert.Equal(expectedSessionIdentificationNumber, actualSessionIdentificationNumber);
            this.chatRepositoryMock.Verify(repository => repository.CreateChatSessionAsync(userIdentificationNumber, issueCategoryName), Times.Once);
        }
    }
}