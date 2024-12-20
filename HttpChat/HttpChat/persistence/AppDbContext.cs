using HttpChat.Model;
using Microsoft.EntityFrameworkCore;

namespace HttpChat.persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Content)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(m => m.ClientId)
                .IsRequired();
            entity.Property(m => m.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.HasMany(c => c.Messages)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
}