using HttpChat.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HttpChat.persistence
{
    public class ChatDbContext : IdentityDbContext<User>
    {
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }

        public ChatDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Chat>()
                .HasMany(c => c.Participants)
                .WithMany(u => u.Chats)
                .UsingEntity(j => j.ToTable("UserChats"));

            modelBuilder.Entity<Chat>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Chat)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Messages)
                .WithOne(m => m.Sender)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Message>()
                .Property(m => m.Content)
                .IsRequired();


            // modelBuilder.Entity<Chat>().HasData(
            //     new Chat { Id = 1, Name = "General Chat", IsGroupChat = true, CreatedAt = DateTime.UtcNow },
            //     new Chat { Id = 2, Name = "Project Team", IsGroupChat = true, CreatedAt = DateTime.UtcNow }
            // );
            //
            // modelBuilder.Entity<Message>().HasData(
            //     new Message { Id = 1, Content = "Welcome to the chat!", SentAt = DateTime.UtcNow, UserId = "1", ChatId = 1 },
            //     new Message { Id = 2, Content = "Hello everyone!", SentAt = DateTime.UtcNow, UserId = "2", ChatId = 1 },
            //     new Message { Id = 3, Content = "Hi team, let's get started.", SentAt = DateTime.UtcNow, UserId = "3", ChatId = 2 }
            // );
            
        }
    }
}

