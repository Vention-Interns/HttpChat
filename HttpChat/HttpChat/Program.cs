using System.Text;
using HttpChat.Model;
using HttpChat.persistence;
using HttpChat.Service.ChatService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ChatDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "http://localhost:5000",
            ValidAudience = "http://localhost:5000",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("sdfgydsfkgksdjfgksdlfkjglksdjfgjkldsflkjglsdjfdsfjghdssdfkjghkdjsfhgj"))
        };
    });

builder.Services.AddScoped<IMessageService, MessageService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("*")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
    var dbContext = serviceProvider.GetRequiredService<ChatDbContext>();
    
    dbContext.Database.EnsureCreated();
    
    var admin = userManager.FindByEmailAsync("admin@example.com").Result;
    if (admin == null)
    {
        admin = new User
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
        john = new User
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
        jane = new User
        {
            Id = "3",
            UserName = "jane_doe",
            Email = "jane.doe@example.com",
            CreatedAt = DateTime.UtcNow
        };
        userManager.CreateAsync(jane, "Jane@123").Wait();
    }
    
    if (!dbContext.Chats.Any())
    {
        var generalChat = new Chat
        {
            Id = 1,
            Name = "General Chat",
            IsGroupChat = true,
            CreatedAt = DateTime.UtcNow,
            Participants = new List<User> { admin, john, jane }
        };

        var projectTeamChat = new Chat
        {
            Id = 2,
            Name = "Project Team",
            IsGroupChat = true,
            CreatedAt = DateTime.UtcNow,
            Participants = new List<User> { admin, jane }
        };

        dbContext.Chats.AddRange(generalChat, projectTeamChat);
        dbContext.SaveChanges();
    }
    
    if (!dbContext.Messages.Any())
    {
        var messages = new List<Message>
        {
            new Message
            {
                Content = "Welcome to the chat!",
                SentAt = DateTime.UtcNow,
                UserId = admin.Id,
                ChatId = 1
            },
            new Message
            {
                Content = "Hello everyone!",
                SentAt = DateTime.UtcNow,
                UserId = john.Id,
                ChatId = 1
            },
            new Message
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.MapControllers();

app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseAuthentication();
app.UseAuthorization();

app.Run();