namespace HttpChat.Model;

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    
    // public int UserId { get; set; }
    // public int ChatId { get; set; }
    //
    // public User Sender { get; set; }
    // public Chat Chat { get; set; }
}
