namespace HttpChat.Model;

public class Chat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public List<Message> Messages { get; set; } = new List<Message>();

    public void AddMessage(Message message)
    {
        Messages.Add(message);
    }

    public void RemoveMessage(Message message)
    {
        Messages.Remove(message);
    }
}