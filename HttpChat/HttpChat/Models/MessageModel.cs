namespace HttpChat.Model
{
    public class MessageModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public string UserId { get; set; } // Changed to string
        public int ChatId { get; set; }
        public UserModel Sender { get; set; }
        public ChatModel Chat { get; set; }
    }
}