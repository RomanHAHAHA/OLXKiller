using Common.Infrastructure.Messaging.Publishers;
using OrdersService.API.Extensions;

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

EventPublisherExtensions.Initialize(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();