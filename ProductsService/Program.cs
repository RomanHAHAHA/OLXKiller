using Common.Infrastructure.Messaging.Publishers;
using Microsoft.Extensions.FileProviders;
using ProductsService.API.Extensions;
using ProductsService.Application.Features.ProductImages.Create;

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

var options = builder.Configuration
    .GetSection(nameof(ProductImagesOptions))
    .Get<ProductImagesOptions>()!;

if (!Directory.Exists(options.Path))
{
    Directory.CreateDirectory(options.Path);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(options.Path),
    RequestPath = "/product-images"
});
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();