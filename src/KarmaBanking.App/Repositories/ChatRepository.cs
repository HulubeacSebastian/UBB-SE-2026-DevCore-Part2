namespace KarmaBanking.App.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using KarmaBanking.App.Data;
    using KarmaBanking.App.Models;
    using KarmaBanking.App.Repositories.Interfaces;
    using Microsoft.Data.SqlClient;
    public class ChatRepository : IChatRepository
    {
        private const int MessageIdOrdinal = 0;
        private const int MessageSessionIdOrdinal = 1;
        private const int MessageSenderTypeOrdinal = 2;
        private const int MessageContentOrdinal = 3;
        private const int MessageSentAtOrdinal = 4;

        private const int SessionIdOrdinal = 0;
        private const int SessionUserIdOrdinal = 1;
        private const int SessionIssueCategoryOrdinal = 2;
        private const int SessionStatusOrdinal = 3;
        private const int SessionRatingOrdinal = 4;
        private const int SessionStartedAtOrdinal = 5;
        private const int SessionEndedAtOrdinal = 6;
        private const int SessionFeedbackOrdinal = 7;

        private const int FeedbackMaxLength = 255;
        private const int SessionStatusMaxLength = 30;
        private const int IssueCategoryMaxLength = 50;
        private const int DefaultSessionRating = 0;

        public async Task<List<ChatMessage>> GetChatMessagesAsync(int chatSessionId)
        {
            var messages = new List<ChatMessage>();

            using (var connection = DatabaseConfig.GetDatabaseConnection())
            {
                await connection.OpenAsync();

                var command = new SqlCommand(
                    "SELECT id, sessionId, senderType, content, sentAt " +
                    "FROM ChatMessage " +
                    "WHERE sessionId = @chatId " +
                    "ORDER BY sentAt",
                    connection);

                command.Parameters.AddWithValue("@chatId", chatSessionId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        messages.Add(new ChatMessage
                        {
                            Id = reader.GetInt32(MessageIdOrdinal),
                            SessionId = reader.GetInt32(MessageSessionIdOrdinal),
                            SenderType = reader.IsDBNull(MessageSenderTypeOrdinal) ? string.Empty : reader.GetString(MessageSenderTypeOrdinal),
                            Content = reader.IsDBNull(MessageContentOrdinal) ? string.Empty : reader.GetString(MessageContentOrdinal),
                            SentAt = reader.GetDateTime(MessageSentAtOrdinal),
                        });
                    }
                }
            }

            return messages;
        }

        public void SaveSessionRatingAndFeedback(int sessionId, int rating, string feedback)
        {
            using (SqlConnection connection = DatabaseConfig.GetDatabaseConnection())
            {
                connection.Open();

                string query = @"UPDATE ChatSession
                             SET rating = @rating,
                                 feedback = @feedback,
                                 sessionStatus = @sessionStatus,
                                 endedAt = @endedAt
                             WHERE id = @sessionId";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.Add("@sessionId", SqlDbType.Int).Value = sessionId;
                command.Parameters.Add("@rating", SqlDbType.Int).Value = rating;
                command.Parameters.Add("@feedback", SqlDbType.NVarChar, FeedbackMaxLength).Value =
                    string.IsNullOrWhiteSpace(feedback) ? DBNull.Value : feedback;
                command.Parameters.Add("@sessionStatus", SqlDbType.NVarChar, SessionStatusMaxLength).Value = "Closed";
                command.Parameters.Add("@endedAt", SqlDbType.DateTime2).Value = DateTime.Now;

                command.ExecuteNonQuery();
            }
        }

        public async Task<int> CreateChatSessionAsync(int userId, string issueCategory)
        {
            using (SqlConnection connection = DatabaseConfig.GetDatabaseConnection())
            {
                await connection.OpenAsync();

                string query = @"INSERT INTO ChatSession (userId, issueCategory, sessionStatus, startedAt)
                                 OUTPUT INSERTED.id
                                 VALUES (@userId, @issueCategory, @sessionStatus, @startedAt)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                    command.Parameters.Add("@issueCategory", SqlDbType.NVarChar, IssueCategoryMaxLength).Value = issueCategory;
                    command.Parameters.Add("@sessionStatus", SqlDbType.NVarChar, SessionStatusMaxLength).Value = "Open";
                    command.Parameters.Add("@startedAt", SqlDbType.DateTime2).Value = DateTime.Now;

                    return (int)await command.ExecuteScalarAsync();
                }
            }
        }

        public async Task AddChatMessageAsync(ChatMessage message)
        {
            using (var connection = DatabaseConfig.GetDatabaseConnection())
            {
                await connection.OpenAsync();
                string query = @"INSERT INTO ChatMessage (sessionId, senderType, content, sentAt) 
                         VALUES (@sessionId, @senderType, @content, @sentAt)";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@sessionId", message.SessionId);
                command.Parameters.AddWithValue("@senderType", message.SenderType);
                command.Parameters.AddWithValue("@content", message.Content);
                command.Parameters.AddWithValue("@sentAt", message.SentAt);

                await command.ExecuteNonQueryAsync();
                await command.ExecuteScalarAsync();
            }
        }

        public async Task<List<ChatSession>> GetChatSessionsAsync()
        {
            var chatSessions = new List<ChatSession>();
            using (SqlConnection connection = DatabaseConfig.GetDatabaseConnection())
            {
                await connection.OpenAsync();

                string query = @"SELECT id, userId, issueCategory, sessionStatus, rating, startedAt, endedAt, feedback " +
                    "FROM ChatSession " +
                    "ORDER BY id DESC";

                SqlCommand command = new SqlCommand(query, connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        chatSessions.Add(new ChatSession
                        {
                            Id = reader.GetInt32(SessionIdOrdinal),
                            UserId = reader.GetInt32(SessionUserIdOrdinal),
                            IssueCategory = reader.IsDBNull(SessionIssueCategoryOrdinal) ? string.Empty : reader.GetString(SessionIssueCategoryOrdinal),
                            SessionStatus = reader.IsDBNull(SessionStatusOrdinal) ? string.Empty : reader.GetString(SessionStatusOrdinal),
                            Rating = reader.IsDBNull(SessionRatingOrdinal) ? DefaultSessionRating : reader.GetInt32(SessionRatingOrdinal),
                            StartedAt = reader.GetDateTime(SessionStartedAtOrdinal),
                            EndedAt = reader.IsDBNull(SessionEndedAtOrdinal) ? DateTime.MinValue : reader.GetDateTime(SessionEndedAtOrdinal),
                            Feedback = reader.IsDBNull(SessionFeedbackOrdinal) ? string.Empty : reader.GetString(SessionFeedbackOrdinal)
                        });
                    }
                }
            }
            return chatSessions;
        }
    }
}