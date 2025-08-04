using Common.Application.Options;
using Common.Domain.Abstractions;
using Common.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Common.API.Filters;

public class IdempotencyFilter<T>(
    ICacheService<object> cache, 
    ILogger<IdempotencyFilter<T>> logger,
    IOptions<ServiceOptions> serviceOptions) : IFilter<ConsumeContext<T>> where T : BaseEvent
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var keyObject = new
        {
            CorrelationId = context.Message.CorrelationId,
            EventType = typeof(T).Name,
            SenderServiceName = context.Message.SenderServiceName,
            ReceiverServiceName = serviceOptions.Value.Name
        };
        var key = JsonConvert.SerializeObject(keyObject);

        var isNew = await cache.SetIfNotExistsAsync(key, new object(), TimeSpan.FromHours(1));

        if (!isNew)
        {
            logger.LogInformation($"Skipping handled message {key}");
            return; 
        }

        logger.LogInformation($"Handle message {key}");
        
        await next.Send(context);
    }

    public void Probe(ProbeContext context) => context.CreateFilterScope("IdempotencyFilter");
}