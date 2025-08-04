using Common.Infrastructure.Messaging.Publishers;
using ReviewsService.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder
    .AddOptionsServices()
    .AddDatabase()
    .AddApplicationServices()
    .AddMessaging();

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