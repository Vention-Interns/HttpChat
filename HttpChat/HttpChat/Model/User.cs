namespace HttpChat.Model;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<Chat> Chats { get; set; }
    public ICollection<Message> Messages { get; set; }
}