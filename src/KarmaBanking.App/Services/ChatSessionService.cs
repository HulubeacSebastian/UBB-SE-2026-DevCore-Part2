using KarmaBanking.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KarmaBanking.App.Services
{
    public class ChatSessionService
    {
        public ChatSession CreateSession(int sessionId)
        {
            ChatSession session = new ChatSession
            {
                IdentificationNumber = sessionId,
                IssueCategory = "General",
                SessionStatus = "Open",
                StartedAt = DateTime.Now,
                Title = $"Session {sessionId}",
            };

            session.Messages.Add(CreateMessage(session, 1, "CHATBOT ASSISTANCE", "Welcome. How can I help you?", DateTime.Now));
            return session;
        }

        public ChatMessage CreateMessage(ChatSession session, int messageId, string senderType, string content, DateTime sentAt)
        {
            return new ChatMessage
            {
                IdentificationNumber = messageId,
                SessionIdentificationNumber = session.IdentificationNumber,
                SenderType = senderType,
                Content = content,
                SentAt = sentAt,
            };
        }

        public string BuildTranscript(ChatSession? session)
        {
            if (session == null)
            {
                return "No chat session selected.";
            }

            List<string> lines = session.Messages
                .Select(message => $"[{message.SentAt:g}] {message.SenderType}: {message.Content}")
                .ToList();

            return string.Join(Environment.NewLine, lines);
        }
    }
}
