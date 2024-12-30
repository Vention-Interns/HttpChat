using Microsoft.AspNetCore.Identity;

namespace HttpChat.Model
{
    public class User : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Chat> Chats { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}