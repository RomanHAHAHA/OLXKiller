using Microsoft.Extensions.FileProviders;
using UsersService.API.Extensions;
using UsersService.Application.Features.Users.SetAvatarImage;

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var options = builder.Configuration
    .GetSection(nameof(UserImagesOptions))
    .Get<UserImagesOptions>()!;

if (!Directory.Exists(options.Path))
{
    Directory.CreateDirectory(options.Path);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(options.Path),
    RequestPath = "/user-images"
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();