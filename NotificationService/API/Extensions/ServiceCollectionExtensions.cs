using Common.API.Extensions;
using Common.API.Filters;
using Common.Application.Options;
using Common.Application.Services;
using Common.Domain.Interfaces;
using MassTransit;
using NotificationService.Application.Services;
using NotificationService.Domain.Interfaces;
using NotificationService.Infrastructure.Consumers;
using StackExchange.Redis;

namespace NotificationService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSignalR();
        
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

        builder.Services.AddScoped(typeof(ICacheService<>), typeof(CacheService<>));
        builder.Services.AddScoped<IHashCacheService, RedisHashCacheService>();
        builder.Services.AddScoped<IRedisLockService, RedisLockService>();
        
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
    
            bugConfigurator.UsingRabbitMq((context, c) =>
            {
                c.UseConsumeFilter(typeof(IdempotencyFilter<>), context);
                
                c.Host(builder.Configuration["MessageBroker:Host"]!, "/", h =>
                {
                    h.Username(builder.Configuration["MessageBroker:UserName"]!);
                    h.Password(builder.Configuration["MessageBroker:Password"]!);
                });
                
                c.ReceiveEndpoint("notifications-product-snapshot-creation-failed", e => e.ConfigureConsumer<ProductSnapshotCreationFailedConsumer>(context));
                c.ReceiveEndpoint("notifications-order-processed", e => e.ConfigureConsumer<OrderProcessedConsumer>(context));
                c.ReceiveEndpoint("notifications-product-reservation-failed", e => e.ConfigureConsumer<ProductsReservationFailedConsumer>(context));

                c.ConfigureEndpoints(context);
            });
        });
        
        return builder;
    }
}