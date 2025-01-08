using HttpChat.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HttpChat.persistence
{
    public class ChatDbContext(DbContextOptions options) : IdentityDbContext<UserModel>(options)
    {
        public DbSet<ChatModel> Chats { get; set; }
        public DbSet<MessageModel> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatModel>()
                .HasMany(c => c.Participants)
                .WithMany(u => u.Chats)
                .UsingEntity(j => j.ToTable("UserChats"));

            modelBuilder.Entity<ChatModel>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Chat)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserModel>()
                .HasMany(u => u.Messages)
                .WithOne(m => m.Sender)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserModel>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<MessageModel>()
                .Property(m => m.Content)
                .IsRequired();
            
        }
    }
}

