using Common.API.Extensions;
using Common.API.Filters;
using Common.Application.Options;
using Common.Application.Services;
using Common.Domain.Interfaces;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using StackExchange.Redis;
using UsersService.Application.Features.Accounts.Login;
using UsersService.Application.Features.Accounts.Register;
using UsersService.Application.Features.Users.GetPagedList;
using UsersService.Application.Features.Users.SetAvatarImage;
using UsersService.Domain.Entities;
using UsersService.Domain.Interfaces;
using UsersService.Infrastructure.Persistence;
using UsersService.Infrastructure.Persistence.Repositories.Base;

namespace UsersService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<UserDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL"));
        });

        builder.Services.AddScoped<IUsersRepository, UsersRepository>();
        
        builder.Services.AddTransient<IFilterStrategy<User, UsersFilter>, UsersFilterStrategy>();
        builder.Services.AddTransient<ISortStrategy<User>, UsersSortStrategy>();

        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
        
        return builder;
    }

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        builder.Services.AddFluentValidationAutoValidation();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IHttpUserContext, HttpUserContext>();
        builder.Services.AddApiAuthorization(builder.Configuration);
        
        builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();
        builder.Services.AddTransient<IJwtProvider, JwtProvider>();
        builder.Services.AddTransient<IFileStorageService, FileStorageService>();
        
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommandHandler).Assembly);
        });
        
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

        builder.Services.AddScoped(typeof(ICacheService<>), typeof(CacheService<>));
        
        return builder;
    }

    public static WebApplicationBuilder AddOptionsServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddConfiguredOptions<JwtOptions>(builder.Configuration);
        builder.Services.AddConfiguredOptions<CustomCookieOptions>(builder.Configuration);
        builder.Services.AddConfiguredOptions<UserImagesOptions>(builder.Configuration);
        builder.Services.AddConfiguredOptions<ServiceOptions>(builder.Configuration);

        return builder;
    }

    public static WebApplicationBuilder AddMessaging(this WebApplicationBuilder builder)
    {
        //builder.Services.AddScoped(typeof(IdempotencyFilter<>));
        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumers(typeof(Program).Assembly);
            x.AddDelayedMessageScheduler();
            x.SetKebabCaseEndpointNameFormatter();
    
            x.AddEntityFrameworkOutbox<UserDbContext>(options =>
            {
                options.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
                options.QueryDelay = TimeSpan.FromSeconds(1);
                options.UseSqlServer().UseBusOutbox();
            });
    
            x.UsingRabbitMq((context, c) =>
            {
                c.UseDelayedMessageScheduler(); 
                c.UseMessageRetry(r =>
                {
                    r.Interval(5, TimeSpan.FromSeconds(5));
                });
                
                c.UseConsumeFilter(typeof(IdempotencyFilter<>), context);
                
                c.Host(builder.Configuration["MessageBroker:Host"]!, "/", h =>
                {
                    h.Username(builder.Configuration["MessageBroker:UserName"]!);
                    h.Password(builder.Configuration["MessageBroker:Password"]!);
                });
        
                c.ConfigureEndpoints(context);
            });
        });
        
        return builder;
    }
}