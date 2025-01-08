using HttpChat.dto;
using HttpChat.dtos;
using HttpChat.Dtos;
using HttpChat.persistence;
using Microsoft.EntityFrameworkCore;

namespace HttpChat.Services.ChatService;

public class ChatService : IChatService
{
    
    private readonly ChatDbContext _appDbContext;

    public ChatService(ChatDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<List<ChatResponseDto>> ReceiveUserChatsAsync(string clientId)
    {
        var chats = await _appDbContext.Chats
            .Where(c => c.Participants.Any(p => p.Id == clientId)).Include(chatModel => chatModel.Participants)
            .ToListAsync();
        
        var response = chats.Select(chat => new ChatResponseDto
        {
            ChatId = chat.Id,
            ChatName = chat.Name,
            IsGroupChat = chat.IsGroupChat,
            NumberOfParticipants = chat.Participants.Count
        }).ToList();

        return response;
    }
    
    public async Task<List<MessageResponseDto>> GetChatMessagesAsync(int chatId)
    {
        // Check if the chat exists
        var chat = await _appDbContext.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId);

        if (chat == null)
        {
            throw new KeyNotFoundException("Chat not found.");
        }

        // Map messages to DTOs
        var messages = chat.Messages
            .Select(m => new MessageResponseDto
            {
                MessageId = m.Id,
                Content = m.Content,
                SenderId = m.UserId,
                SentAt = m.SentAt
            })
            .OrderBy(m => m.SentAt)
            .ToList();

        return messages;
    }

}