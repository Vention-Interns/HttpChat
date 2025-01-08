using HttpChat.dtos;
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
            .Where(c => c.Participants.Any(p => p.Id == clientId))
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

}