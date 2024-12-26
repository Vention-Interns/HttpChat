using HttpChat.Model;
using Microsoft.EntityFrameworkCore;

namespace HttpChat.persistence;

public class ChatDbContext : DbContext
{

    // public DbSet<User> Users { get; set; }
    // public DbSet<Chat> Chats { get; set; }
    public DbSet<Message> Messages { get; set; }

    public ChatDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // modelBuilder.Entity<Chat>()
        //     .HasMany(c => c.Participants)
        //     .WithMany(u => u.Chats)
        //     .UsingEntity(j => j.ToTable("UserChats"));
        //
        // modelBuilder.Entity<Chat>()
        //     .HasMany(c => c.Messages)
        //     .WithOne(m => m.Chat)
        //     .HasForeignKey(m => m.ChatId);
        //
        // modelBuilder.Entity<User>()
        //     .HasMany(u => u.Messages)
        //     .WithOne(m => m.Sender)
        //     .HasForeignKey(m => m.UserId);
    }
    
}