using HttpChat.dto;
using HttpChat.persistence;

namespace HttpChat.Service.ChatService;

public class ChatService : IChatService
{
    private readonly AppDbContext _appDbContext;

    public ChatService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public void SaveLocalMessages(List<MessageRequestDto> messages)
    {
        var messageEntities =
            messages.Select(MessageRequestDto.ToEntity).ToList();
        
        _appDbContext.Messages.AddRange(messageEntities);
        
        _appDbContext.SaveChanges();
    }
    
}
