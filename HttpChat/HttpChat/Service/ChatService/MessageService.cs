using HttpChat.dto;
using HttpChat.persistence;

namespace HttpChat.Service.ChatService;

public class MessageService : IMessageService
{
    private readonly ChatDbContext _appDbContext;

    public MessageService(ChatDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public void SaveLocalMessages(IEnumerable<MessageRequestDto> messages)
    {
        var messageEntities =
            messages.Select(MessageRequestDto.ToEntity).ToList();
        
        _appDbContext.Messages.AddRange(messageEntities);
        
        _appDbContext.SaveChanges();
    }
    
}
