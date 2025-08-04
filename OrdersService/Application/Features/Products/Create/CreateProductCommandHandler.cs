using Common.Application.Options;
using Common.Domain.Entities;
using Common.Infrastructure.Messaging.Events.Product;
using Common.Infrastructure.Messaging.Publishers;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using OrdersService.Domain.Interfaces;
using OrdersService.Infrastructure.Persistence;

namespace OrdersService.Application.Features.Products.Create;

public class CreateProductCommandHandler(
    IProductsRepository productsRepository,
    IPublishEndpoint publisher,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<CreateProductCommand>
{
    public async Task Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        await productsRepository.CreateAsync(
            new ProductSnapshot
            {
                Id = request.ProductId,
                Name = request.Name,
                Price = request.Price,
            }, 
            cancellationToken);
        
        try
        {
            await OnProductCreated(request, cancellationToken);
        }
        catch
        {
            await OnProductCreationFailed(request, cancellationToken);
        }
    }
    
    private async Task OnProductCreated(CreateProductCommand request, CancellationToken cancellationToken)
    {
        await publisher.Publish(
            new ProductSnapshotCreatedEvent
            {
                CorrelationId = request.CorrelationId,
                SenderServiceName = serviceOptions.Value.Name,
                ProductId = request.ProductId,
                UserId = request.SellerId
            }, 
            cancellationToken);
        
        await productsRepository.SaveChangesAsync(cancellationToken);
    }
    
    private async Task OnProductCreationFailed(CreateProductCommand request, CancellationToken cancellationToken)
    {
        await publisher.PublishInIsolatedScopeAsync<OrdersDbContext>(
            new ProductSnapshotCreationFailedEvent
            {
                CorrelationId = request.CorrelationId,
                SenderServiceName = serviceOptions.Value.Name,
                ProductId = request.ProductId,
                UserId = request.SellerId,
            },
            cancellationToken);
    }
}