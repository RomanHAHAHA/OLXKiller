using Common.API.Extensions;
using Common.API.Filters;
using Common.Application.Options;
using Common.Application.Services;
using Common.Domain.Interfaces;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ReviewsService.Application.Features.Reviews.Create;
using ReviewsService.Application.Features.Reviews.GetFilteredReviews;
using ReviewsService.Application.Services;
using ReviewsService.Domain.Entities;
using ReviewsService.Domain.Interfaces;
using ReviewsService.Infrastructure.Events.Consumers;
using ReviewsService.Infrastructure.Persistence;
using ReviewsService.Infrastructure.Persistence.Repositories;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using StackExchange.Redis;

namespace ReviewsService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IReviewsRepository, ReviewsRepository>();
        builder.Services.AddScoped<IFilterStrategy<Review, ReviewsFilter>, ReviewsFilterStrategy>();
        builder.Services.AddScoped<ISortStrategy<Review>, ReviewSortStrategy>();
        
        builder.Services.AddScoped<IUsersRepository, UsersRepository>();
        builder.Services.AddScoped<IProductsRepository, ProductsRepository>();

        builder.Services.AddDbContext<ReviewsDbContext>(options =>
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
        
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IHttpUserContext, HttpUserContext>();
        builder.Services.AddHttpClient<IOrderServiceClient, OrderServiceClient>();
        
        builder.Services.AddApiAuthorization(builder.Configuration);

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateReviewCommandHandler).Assembly);
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
        builder.Services.AddConfiguredOptions<ServiceOptions>(builder.Configuration);

        return builder;
    }

    public static WebApplicationBuilder AddMessaging(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped(typeof(IdempotencyFilter<>));
        
        builder.Services.AddMassTransit(bc =>
        {
            bc.AddConsumers(typeof(Program).Assembly);
            bc.SetKebabCaseEndpointNameFormatter();

            bc.AddEntityFrameworkOutbox<ReviewsDbContext>(o =>
            {
                o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
                o.QueryDelay = TimeSpan.FromSeconds(1);
                o.UseSqlServer(); 
                o.UseBusOutbox();
            });

            bc.UsingRabbitMq((context, c) =>
            {
                c.UseConsumeFilter(typeof(IdempotencyFilter<>), context);
                
                c.Host(builder.Configuration["MessageBroker:Host"]!, "/", h =>
                {
                    h.Username(builder.Configuration["MessageBroker:UserName"]!);
                    h.Password(builder.Configuration["MessageBroker:Password"]!);
                });

                c.ConfigureEndpoints(context);

                c.ReceiveEndpoint("reviews-product-created", e => e.ConfigureConsumer<ProductCreatedConsumer>(context));
                c.ReceiveEndpoint("reviews-product-updated", e => e.ConfigureConsumer<ProductUpdatedConsumer>(context));
                c.ReceiveEndpoint("reviews-product-main-image-set", e => e.ConfigureConsumer<ProductMainImageSetConsumer>(context));
                c.ReceiveEndpoint("reviews-product-deleted", e => e.ConfigureConsumer<ProductDeletedConsumer>(context));
                c.ReceiveEndpoint("reviews-user-avatar-updated", e => e.ConfigureConsumer<UserAvatarUpdatedConsumer>(context));
                c.ReceiveEndpoint("reviews-user-registered", e => e.ConfigureConsumer<UserRegisteredConsumer>(context));
                c.ReceiveEndpoint("reviews-user-deleted", e => e.ConfigureConsumer<UserDeletedConsumer>(context));
                c.ReceiveEndpoint("reviews-user-avatar-rollback", e => e.ConfigureConsumer<UserAvatarRollBackConsumer>(context));
            });
        });
        
        return builder;
    }
}