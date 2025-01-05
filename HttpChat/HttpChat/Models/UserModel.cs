using Microsoft.AspNetCore.Identity;

namespace HttpChat.Model
{
    public class UserModel : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ChatModel> Chats { get; set; }
        public ICollection<MessageModel> Messages { get; set; }
    }
}