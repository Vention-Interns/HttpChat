using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace HttpChat.Model
{
    public class User : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Chat> Chats { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}