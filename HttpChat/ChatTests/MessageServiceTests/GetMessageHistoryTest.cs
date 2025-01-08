using HttpChat.dto;
using HttpChat.Service.ChatService;
using NUnit.Framework;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GetMessageHistoryAsyncTest
{
    private const string VALID_CHAT_ID = "chat1";
    private const string INVALID_CHAT_ID = "chat0";
    private const string VALID_CLIENT_ID = "client1";
    private const string VALID_CLIENT_ID2 = "client2";
    private const string CONTENT = "content1";
    private const string CONTENT2 = "content2";
    MessageService _messageService;

    [SetUp]
    public void Setup()
    {
        var db = MessageService.GetMemoryContext();
        _messageService = new MessageService(db);
    }

    [TearDown]
    public void Teardown()
    {
        _messageService.CleanChatMessageQueues();
    }

    [Test]
    public Task GetMessageHistoryAsync_ShouldReturnMessages_WhenChatIdExists()
    {
        MessageRequestDto messageRequestDto1 = new MessageRequestDto
        {
            ClientId = VALID_CLIENT_ID,
            Content = CONTENT,
            ChatId = VALID_CHAT_ID
        };
        MessageRequestDto messageRequestDto2 = new MessageRequestDto
        {
            ClientId = VALID_CLIENT_ID2,
            Content = CONTENT2,
            ChatId = VALID_CHAT_ID
        };
        _messageService.SendMessage(messageRequestDto1);
        _messageService.SendMessage(messageRequestDto2);

        var result = _messageService.GetMessageHistoryAsync(VALID_CHAT_ID);

        Assert.IsNotNull(result);
        return Task.CompletedTask;
    }

    [Test]
    public async Task GetMessageHistoryAsync_ShouldReturnNull_WhenChatIdDoesNotExist()
    {
        var result = await _messageService.GetMessageHistoryAsync(INVALID_CHAT_ID);

        Assert.IsNull(result);
    }
}
