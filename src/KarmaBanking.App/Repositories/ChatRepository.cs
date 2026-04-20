using KarmaBanking.App.Models;
using KarmaBanking.App.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace KarmaBanking.App.Repositories
{
    public class ChatRepository : IChatRepository
    {
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
                    connection
                );

                command.Parameters.AddWithValue("@chatId", chatSessionId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        messages.Add(new ChatMessage
                        {
                            Id = reader.GetInt32(0),
                            SessionId = reader.GetInt32(1),
                            SenderType = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Content = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            SentAt = reader.GetDateTime(4)
                        });
                    }
                }
            }

            return messages;
        }

        public void SaveSessionRatingAndFeedback(int sessionId, int rating, string feedback)
        {
            using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
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
                command.Parameters.Add("@feedback", SqlDbType.NVarChar, 255).Value =
                    string.IsNullOrWhiteSpace(feedback) ? DBNull.Value : feedback;
                command.Parameters.Add("@sessionStatus", SqlDbType.NVarChar, 30).Value = "Closed";
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
                    command.Parameters.Add("@issueCategory", SqlDbType.NVarChar, 50).Value = issueCategory;
                    command.Parameters.Add("@sessionStatus", SqlDbType.NVarChar, 30).Value = "Open";
                    command.Parameters.Add("@startedAt", SqlDbType.DateTime2).Value = DateTime.Now;

                    return (int)await command.ExecuteScalarAsync();
                }
            }
        }

        public async Task<List<ChatSession>> GetChatSessionsAsync()
        {
            var chatSessions = new List<ChatSession>();
            using (SqlConnection connection = DatabaseConfig.GetDatabaseConnection())
            {
                await connection.OpenAsync();

                string query = @"SELECT is, userId, issueCategory, sessionStatus, rating, startedAt, endedAt, feedback " +
                    "FROM ChatSession";

                SqlCommand command = new SqlCommand(query, connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        chatSessions.Add(new ChatSession
                        {
                            Id = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            IssueCategory = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            SessionStatus = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Rating = reader.GetInt32(4),
                            StartedAt = reader.GetDateTime(5),
                            EndedAt = reader.GetDateTime(6),
                            Feedback = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                        });
                    }
                }
            }
            return chatSessions;
        }
    }
}