namespace KarmaBanking.App.Repositories.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using KarmaBanking.App.Models;

public interface IChatRepository
{
    Task<List<ChatMessage>> GetChatMessagesAsync(int chatSessionId);
}