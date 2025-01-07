using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using HttpChat.dto;
using HttpChat.Service.ChatService;

namespace ChatTests.MessageServiceTests
{
    public class RegisterUserTest
    {
        private const string VALID_CHAT_ID = "chat1";
        private const string VALID_CLIENT_ID = "client1";
        MessageService _messageService;
        RegisterUserRequest _registerUserRequest;

        [SetUp]
        public void Setup()
        {
            var db = MessageService.GetMemoryContext();
            _messageService = new MessageService(db);
            _registerUserRequest = new RegisterUserRequest
            {
                ClientId = VALID_CLIENT_ID,
                ChatId = VALID_CHAT_ID
            };
        }

        [Test]
        public void RegisterUserShouldAddUserToChatClientQueues() 
        {
            _messageService.RegisterUser(_registerUserRequest);
            bool result = _messageService.IsChatIdAndClientIdCompatible(_registerUserRequest.ChatId, _registerUserRequest.ClientId);
            Assert.IsTrue(result);
        }

    }
}
