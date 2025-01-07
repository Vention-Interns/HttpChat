using HttpChat.Model;
using Microsoft.AspNetCore.Identity;
using HttpChat.persistence;

public static class DbDataGenerator
{
    public static void GenerateFakeData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        var userManager = scopedProvider.GetRequiredService<UserManager<UserModel>>();
        var dbContext = scopedProvider.GetRequiredService<ChatDbContext>();

        dbContext.Database.EnsureCreated();

        CreateDefaultUsers(userManager);
        CreateDefaultChatsAndMessages(dbContext, userManager);
    }

    private static void CreateDefaultUsers(UserManager<UserModel> userManager)
    {
        var admin = userManager.FindByEmailAsync("admin@example.com").Result;
        if (admin == null)
        {
            admin = new UserModel
            {
                Id = "1",
                UserName = "admin",
                Email = "admin@example.com",
                CreatedAt = DateTime.UtcNow
            };
            userManager.CreateAsync(admin, "Admin@123").Wait();
        }

        var john = userManager.FindByEmailAsync("john.doe@example.com").Result;
        if (john == null)
        {
            john = new UserModel
            {
                Id = "2",
                UserName = "john_doe",
                Email = "john.doe@example.com",
                CreatedAt = DateTime.UtcNow
            };
            userManager.CreateAsync(john, "John@123").Wait();
        }

        var jane = userManager.FindByEmailAsync("jane.doe@example.com").Result;
        if (jane == null)
        {
            jane = new UserModel
            {
                Id = "3",
                UserName = "jane_doe",
                Email = "jane.doe@example.com",
                CreatedAt = DateTime.UtcNow
            };
            userManager.CreateAsync(jane, "Jane@123").Wait();
        }
    }

    private static void CreateDefaultChatsAndMessages(ChatDbContext dbContext, UserManager<UserModel> userManager)
    {
        var admin = userManager.FindByEmailAsync("admin@example.com").Result;
        var john = userManager.FindByEmailAsync("john.doe@example.com").Result;
        var jane = userManager.FindByEmailAsync("jane.doe@example.com").Result;

        if (!dbContext.Chats.Any())
        {
            var generalChat = new ChatModel
            {
                Id = 1,
                Name = "General Chat",
                IsGroupChat = true,
                CreatedAt = DateTime.UtcNow,
                Participants = new List<UserModel> { admin, john, jane }
            };

            var projectTeamChat = new ChatModel
            {
                Id = 2,
                Name = "Project Team",
                IsGroupChat = true,
                CreatedAt = DateTime.UtcNow,
                Participants = new List<UserModel> { admin, jane }
            };

            dbContext.Chats.AddRange(generalChat, projectTeamChat);
            dbContext.SaveChanges();
        }

        if (!dbContext.Messages.Any())
        {
            var messages = new List<MessageModel>
            {
                new MessageModel
                {
                    Content = "Welcome to the chat!",
                    SentAt = DateTime.UtcNow,
                    UserId = admin.Id,
                    ChatId = 1
                },
                new MessageModel
                {
                    Content = "Hello everyone!",
                    SentAt = DateTime.UtcNow,
                    UserId = john.Id,
                    ChatId = 1
                },
                new MessageModel
                {
                    Content = "Hi team, let's get started.",
                    SentAt = DateTime.UtcNow,
                    UserId = jane.Id,
                    ChatId = 2
                }
            };

            dbContext.Messages.AddRange(messages);
            dbContext.SaveChanges();
        }
    }
}
