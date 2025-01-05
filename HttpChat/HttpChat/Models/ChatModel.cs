namespace HttpChat.Model;

public class ChatModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsGroupChat { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<UserModel> Participants { get; set; } = new List<UserModel>();
    public ICollection<MessageModel> Messages { get; set; }
}