using CartsService.Domain.Interfaces;
using CartsService.Infrastructure.Eventing.Consumers;
using CartsService.Infrastructure.Persistence;
using CartsService.Infrastructure.Persistence.Repositories;
using Common.API.Extensions;
using Common.API.Filters;
using Common.Application.Options;
using Common.Application.Services;
using Common.Domain.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace CartsService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IProductsRepository, ProductsesRepository>();
        builder.Services.AddScoped<ICartsRepository, CartsRepository>();

        builder.Services.AddDbContext<CartsDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL"));
        });

        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
        
        return builder;
    }

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

        builder.Services.AddScoped(typeof(ICacheService<>), typeof(CacheService<>));
        
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
    
            bugConfigurator.AddEntityFrameworkOutbox<CartsDbContext>(options =>
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
                c.ReceiveEndpoint("carts-product-created", e => e.ConfigureConsumer<ProductCreatedConsumer>(context));
                c.ReceiveEndpoint("carts-product-updated", e => e.ConfigureConsumer<ProductUpdatedConsumer>(context));
                c.ReceiveEndpoint("carts-product-deleted", e => e.ConfigureConsumer<ProductDeletedConsumer>(context));
                c.ReceiveEndpoint("carts-product-main-image-set", e => e.ConfigureConsumer<ProductMainImageSetConsumer>(context));
                c.ReceiveEndpoint("carts-order-processed", e => e.ConfigureConsumer<OrderProcessedConsumer>(context));
            });
        });
        
        return builder;
    }
}