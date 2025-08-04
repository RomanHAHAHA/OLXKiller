using Common.API.Filters;
using Common.Application.Options;
using Common.Application.Services;
using Common.Domain.Interfaces;
using EmailService.Application.Common.Options;
using EmailService.Application.Common.Services;
using EmailService.Application.Features.EmailConfirmations.ConfirmEmail;
using EmailService.Application.Features.EmailConfirmations.SendCode;
using EmailService.Domain.Interfaces;
using MassTransit;
using StackExchange.Redis;

namespace EmailService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
        builder.Services.AddTransient<IVerificationCodeGenerator, VerificationCodeGenerator>();
        builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();

        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

        builder.Services.AddScoped(typeof(ICacheService<>), typeof(CacheService<>));

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ConfirmEmailCommandHandler).Assembly);
        });

        return builder;
    }

    public static WebApplicationBuilder AddOptionsServices(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(nameof(SmtpOptions)));
        builder.Services.Configure<ServiceOptions>(builder.Configuration.GetSection(nameof(ServiceOptions)));
        
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
                
                c.ConfigureEndpoints(context);
            });
        });
        
        return builder;
    }
}