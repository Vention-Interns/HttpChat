namespace HttpChat.Model;

public class Message
{
    public Guid Id { get; set; }
    
    public string Content { get; set; }
    
    public string ClientId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
