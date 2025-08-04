using Common.Domain.Abstractions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Common.Infrastructure.Messaging.Publishers;

public static class EventPublisherExtensions
{
    private static IServiceScopeFactory _scopeFactory = null!;

    public static void Initialize(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    }
    
    public static async Task PublishInIsolatedScopeAsync<TDbContext>(
        this IPublishEndpoint _,
        BaseEvent @event, 
        CancellationToken cancellationToken) 
        where TDbContext : DbContext 
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        await retryPolicy.ExecuteAsync(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var scopedPublisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        
            await scopedPublisher.Publish((dynamic)@event, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        });
    }
}