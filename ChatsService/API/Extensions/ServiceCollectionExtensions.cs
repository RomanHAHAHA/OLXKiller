using ChatsService.Application.Services;
using ChatsService.Domain.Interfaces;
using ChatsService.Infrastructure.Consumers;
using ChatsService.Infrastructure.Persistence;
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

namespace ChatsService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ChatsDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL"));
        });

        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
        
        return builder;
    }

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSignalR();
        
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IHttpUserContext, HttpUserContext>();
        
        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        builder.Services.AddFluentValidationAutoValidation();
        
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

        builder.Services.AddScoped(typeof(ICacheService<>), typeof(CacheService<>));
        builder.Services.AddScoped<IChatConnectionTracker, ChatConnectionTracker>();
        
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });

        builder.Services.AddApiAuthorization(builder.Configuration);

        return builder;
    }

    public static WebApplicationBuilder AddOptionsServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddConfiguredOptions<JwtOptions>(builder.Configuration);
        builder.Services.AddConfiguredOptions<CustomCookieOptions>(builder.Configuration);
        builder.Services.AddConfiguredOptions<ServiceOptions>(builder.Configuration);

        return builder;
    }

    public static WebApplicationBuilder AddMessaging(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped(typeof(IdempotencyFilter<>));
        
        builder.Services.AddMassTransit(bugConfigurator =>
        {
            bugConfigurator.AddConsumers(typeof(Program).Assembly);
            bugConfigurator.SetKebabCaseEndpointNameFormatter();
    
            bugConfigurator.AddEntityFrameworkOutbox<ChatsDbContext>(options =>
            {
                options.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
                options.QueryDelay = TimeSpan.FromSeconds(1);
                options.UseSqlServer();
                options.UseBusOutbox();
            });
    
            bugConfigurator.UsingRabbitMq((context, c) =>
            {
                c.UseConsumeFilter(typeof(IdempotencyFilter<>), context);
                
                c.Host(builder.Configuration["MessageBroker:Host"]!, "/", h =>
                {
                    h.Username(builder.Configuration["MessageBroker:UserName"]!);
                    h.Password(builder.Configuration["MessageBroker:Password"]!);
                });
        
                c.ConfigureEndpoints(context);
               
                c.ReceiveEndpoint("chats-user-avatar-updated", e => e.ConfigureConsumer<UserAvatarUpdatedConsumer>(context));
                c.ReceiveEndpoint("chats-user-registered", e => e.ConfigureConsumer<UserRegisteredConsumer>(context));
                c.ReceiveEndpoint("chats-user-deleted", e => e.ConfigureConsumer<UserDeletedConsumer>(context));
                c.ReceiveEndpoint("chats-user-avatar-rollback", e => e.ConfigureConsumer<UserAvatarRollBackConsumer>(context));
            });
        });
        
        return builder;
    }
}