// <copyright file="ChatCategoryServiceTests.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Tests.Services
{
    using global::KarmaBanking.App.Services;
    using Xunit;

    public class ChatCategoryServiceTests
    {
        [Theory]
        [InlineData("I forgot my password", "Account")]
        [InlineData("I lost my card", "Cards")]
        [InlineData("How to transfer money?", "Transfers")]
        [InlineData("TECHNICAL support", "Technical Issue")]
        [InlineData("Hello there", "Other")]
        public void InferCategory_WhenGivenUserQuestion_ThenReturnsExpectedCategory(string userQuestionText, string expectedCategoryName)
        {
            // Arrange
            var chatCategoryService = new ChatCategoryService();

            // Act
            string actualCategoryName = chatCategoryService.InferCategory(userQuestionText);

            // Assert
            Assert.Equal(expectedCategoryName, actualCategoryName);
        }
    }
}