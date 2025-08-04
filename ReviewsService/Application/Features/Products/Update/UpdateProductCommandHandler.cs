using Common.Application.Options;
using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using ReviewsService.Domain.Interfaces;
using ReviewsService.Infrastructure.Persistence;

namespace ReviewsService.Application.Features.Products.Update;

public class UpdateProductCommandHandler(
    IProductsRepository productRepository,
    IPublishEndpoint publisher,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            await OnProductUpdated(request, cancellationToken);
            return;
        }
        
        product.Name = request.Name;
        product.Price = request.Price;

        try
        {
            await OnProductUpdated(request, cancellationToken);
        }
        catch
        {
            await OnProductUpdateFailed(request, cancellationToken);
        }
    }

    private async Task OnProductUpdated(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        await publisher.Publish(
            new ProductSnapshotUpdatedEvent
            {
                CorrelationId = request.CorrelationId,
                SenderServiceName = serviceOptions.Value.Name,
                UserId = request.UserId,
                ProductId = request.ProductId,
            },
            cancellationToken);
        
        await productRepository.SaveChangesAsync(cancellationToken);
    }
    
    private async Task OnProductUpdateFailed(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        await publisher.Publish(
            new ProductSnapshotUpdateFailedEvent()
            {
                CorrelationId = request.CorrelationId,
                SenderServiceName = serviceOptions.Value.Name,
                ProductId = request.ProductId,
            },
            typeof(ReviewsDbContext),
            cancellationToken);
    }
}