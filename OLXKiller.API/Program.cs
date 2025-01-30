using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using OLXKiller.API.Authentication;
using OLXKiller.API.Extensions;
using OLXKiller.API.Middlewares;
using OLXKiller.Application.Abstractions;
using OLXKiller.Application.Factories;
using OLXKiller.Application.Options;
using OLXKiller.Application.Providers;
using OLXKiller.Application.Services;
using OLXKiller.Application.Validators;
using OLXKiller.Domain.Abstractions.Providers;
using OLXKiller.Domain.Abstractions.Repositories;
using OLXKiller.Domain.Entities;
using OLXKiller.Persistence.Contexts;
using OLXKiller.Persistence.Repositories;
using ProjectX.Infrastructure.Repositories;

//TODO: add frontend (create product, add images, collection page with sorting and filter)

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFluentValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<LoginUserValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

#region Options
builder.Services.Configure<DbOptions>(builder.Configuration.GetSection(nameof(DbOptions)));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
builder.Services.Configure<CustomCookieOptions>(builder.Configuration.GetSection(nameof(CustomCookieOptions)));
builder.Services.Configure<ImageManagerOptions>(builder.Configuration.GetSection(nameof(ImageManagerOptions)));
#endregion

#region Repositories
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IRepository<UserAvatarEntity>, UserAvatarsRepository>();
builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
builder.Services.AddScoped<IProductImagesRepository, ProductImagesRepository>();
builder.Services.AddScoped<IRolesRepository, RolesRepository>();
#endregion

#region Providers
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IImageManager, ImageManager>();
#endregion

#region Services
builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IProductsService, ProductsService>();
builder.Services.AddScoped<IPermissionsService, PermissionsService>();
builder.Services.AddScoped<IRolesService, RolesService>();

builder.Services.AddScoped<IProductDtoFactory, ProductDtoFactory>();
#endregion

#region DB
var dbOptions = builder.Configuration
    .GetSection(nameof(DbOptions))
    .Get<DbOptions>() ?? throw new NullReferenceException(nameof(DbOptions));

builder.Services.AddSqlServer<AppDbContext>(dbOptions.ConnectionString);
#endregion

#region Authentication & Authorization
var jwtOptions = builder.Configuration
    .GetSection(nameof(JwtOptions))
    .Get<JwtOptions>() ?? throw new NullReferenceException(nameof(JwtOptions));

var customCookieOptions = builder.Configuration
    .GetSection(nameof(CustomCookieOptions))
    .Get<CustomCookieOptions>() ?? throw new NullReferenceException(nameof(CustomCookieOptions));

builder.Services.AddApiAuthentication(jwtOptions, customCookieOptions);
builder.Services.AddAuthorization();

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
#endregion

builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();
builder.Services.AddTransient<RequestLoggingMiddleware>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:3000")
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseCors();

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
