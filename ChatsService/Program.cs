using ChatsService.API.Extensions;
using ChatsService.API.Hubs;
using Common.Infrastructure.Messaging.Publishers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder
    .AddDatabase()
    .AddMessaging()
    .AddApplicationServices()
    .AddOptionsServices();

var app = builder.Build();

app.MapHub<ChatHub>("/chats-hub");

EventPublisherExtensions.Initialize(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();