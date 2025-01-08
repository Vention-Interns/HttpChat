using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpChat.dto;
using HttpChat.Service.ChatService;

namespace ChatTests.MessageServiceTests
{
    public class SendMessageTest
    {
        private const string VALID_CHAT_ID = "chat1";
        private const string VALID_CLIENT_ID = "client1";
        private const string CONTENT = "content1";
        MessageService _messageService;
        MessageRequestDto _messageRequestDto; 
        
        [SetUp]
        public void Setup()
        {
            var db = MessageService.GetMemoryContext();
            _messageService = new MessageService(db);
            _messageRequestDto = new MessageRequestDto
            {
                ClientId = VALID_CLIENT_ID,
                Content = CONTENT,
                ChatId = VALID_CHAT_ID
            };
        }

        [Test]
        public void SendMessageShouldAddMessageToChatQueue() { 
            _messageService.SendMessage(_messageRequestDto);
            bool isMessageInChat = _messageService.IsChatIdAndMessageCompatible(_messageRequestDto.ChatId, _messageRequestDto);
            Assert.IsTrue(isMessageInChat);
        }
    }
}
