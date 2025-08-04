using Common.API.Extensions;
using Common.API.Filters;
using Common.Application.Options;
using Common.Application.Services;
using Common.Domain.Interfaces;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrdersService.Application.Features.Orders.Create;
using OrdersService.Application.Features.Orders.GetPagedOrders;
using OrdersService.Application.Features.OrderStatusNotifications;
using OrdersService.Application.Features.OrderStatusNotifications.Strategies;
using OrdersService.Application.Options;
using OrdersService.Application.Services;
using OrdersService.Domain.Interfaces;
using OrdersService.Infrastructure.Messaging.Consumers;
using OrdersService.Infrastructure.Persistence;
using OrdersService.Infrastructure.Persistence.Repositories;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using StackExchange.Redis;
using Order = OrdersService.Domain.Entities.Order;

namespace OrdersService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();
        builder.Services.AddScoped<IFilterStrategy<Order, OrderFilter>, OrdersFilterStrategy>();
        builder.Services.AddScoped<ISortStrategy<Order>, OrdersSortStrategy>();
        
        builder.Services.AddScoped<IProductsRepository, ProductsesRepository>();
        builder.Services.AddScoped<IUsersRepository, UsersRepository>();
        
        builder.Services.AddDbContext<OrdersDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL"));
        });

        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
        
        return builder;
    }

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        builder.Services.AddFluentValidationAutoValidation();

        builder.Services.AddHttpClient<ICartServiceClient, CartServiceClient>();
        builder.Services.AddHttpClient<INovaPoshtaClient, NovaPoshtaClient>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IHttpUserContext, HttpUserContext>();
        builder.Services.AddApiAuthorization(builder.Configuration);
        
        builder.Services.AddTransient<IOrderStatusNotificationStrategyFactory, OrderStatusNotificationStrategyFactory>();
        
        builder.Services.AddTransient<OrderPlacedNotificationStrategy>();
        builder.Services.AddTransient<OrderShippedNotificationStrategy>();
        builder.Services.AddTransient<OrderDeliveredNotificationStrategy>();
        builder.Services.AddTransient<OrderPayedNotificationStrategy>();
        builder.Services.AddTransient<OrderCanceledNotificationStrategy>();
        builder.Services.AddTransient<OrderFailedNotificationStrategy>();
        
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommandHandler).Assembly);
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
        builder.Services.AddConfiguredOptions<NovaPoshtaOptions>(builder.Configuration);
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
            
            bugConfigurator.AddEntityFrameworkOutbox<OrdersDbContext>(options =>
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
                
                c.ReceiveEndpoint("orders-product-created", e => e.ConfigureConsumer<ProductCreatedConsumer>(context));
                c.ReceiveEndpoint("orders-product-updated", e => e.ConfigureConsumer<ProductUpdatedConsumer>(context));
                c.ReceiveEndpoint("orders-product-deleted", e => e.ConfigureConsumer<ProductDeletedConsumer>(context));
                c.ReceiveEndpoint("orders-product-main-image-set", e => e.ConfigureConsumer<ProductMainImageSetConsumer>(context));
                c.ReceiveEndpoint("orders-product-reservation-failed", e => e.ConfigureConsumer<ProductsReservationFailedConsumer>(context));
                c.ReceiveEndpoint("orders-user-avatar-updated", e => e.ConfigureConsumer<UserAvatarUpdatedConsumer>(context));
                c.ReceiveEndpoint("orders-user-registered", e => e.ConfigureConsumer<UserRegisteredConsumer>(context));
                c.ReceiveEndpoint("orders-user-deleted", e => e.ConfigureConsumer<UserDeletedConsumer>(context));
                c.ReceiveEndpoint("orders-user-avatar-rollback", e => e.ConfigureConsumer<UserAvatarRollBackConsumer>(context));
            });
        });
        
        return builder;
    }
}