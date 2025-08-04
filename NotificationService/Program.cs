using NotificationService.API.Extensions;
using NotificationService.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder
    .AddMessaging()
    .AddApplicationServices()
    .AddOptionsServices();

var app = builder.Build();

app.MapHub<NotificationHub>("notifications-hub");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();