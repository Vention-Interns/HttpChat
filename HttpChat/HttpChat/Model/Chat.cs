namespace HttpChat.Model;

public class Chat
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsGroupChat { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<User> Participants { get; set; } = new List<User>();
    public ICollection<Message> Messages { get; set; }
}