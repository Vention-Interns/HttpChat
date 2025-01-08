using HttpChat.dto;
using System.Collections.Concurrent;
using HttpChat.persistence;
using HttpChat.Service.ChatService;
using Moq;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace ChatTests.MessageServiceTests
{
    public class IsChatIdValidTest
    {
        private const string VALID_CHAT_ID = "chat1";
        private const string INVALID_CHAT_ID = "chat0";
        MessageService _messageService;

        [SetUp]
        public void Setup()
        {
            var db = MessageService.GetMemoryContext();
            _messageService = new MessageService(db);
            _messageService.CleanChatMessageQueues();
        }

        [TearDown]
        public void Teardown()
        {
            _messageService.CleanChatMessageQueues();
        }

        [Test]
        public async Task IsChatIdValid_ShouldReturnTrue_WhenIdIsValid()
        {
            _messageService.AddChatToChatMessageQueues(VALID_CHAT_ID);
            var result = await _messageService.IsChatIdValid(VALID_CHAT_ID);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task IsChatIdValid_ShouldReturnFalse_WhenThereIsNoChats()
        {
            var result = await _messageService.IsChatIdValid(INVALID_CHAT_ID);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task IsChatIdValid_ShouldReturnFalse_WhenThereIsNoChatWithSuchId()
        {
            _messageService.AddChatToChatMessageQueues(VALID_CHAT_ID);
            var result = await _messageService.IsChatIdValid(INVALID_CHAT_ID);
            Assert.IsFalse(result);
        }

    }
}