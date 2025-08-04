using Common.Application.Options;
using Common.Infrastructure.Messaging.Events.Product;
using Common.Infrastructure.Messaging.Publishers;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using OrdersService.Domain.Interfaces;
using OrdersService.Infrastructure.Persistence;

namespace OrdersService.Application.Features.Products.SetMainImage;

public class SetMainProductImageCommandHandler(
    IProductsRepository productsRepository,
    IPublishEndpoint publisher,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<SetMainProductImageCommand>
{
    public async Task Handle(SetMainProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await productsRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            await OnMainImageSetFailed(request, cancellationToken);
            return;
        }
        
        product.MainImagePath = request.ImagePath;

        try
        {
            await OnMainImageSet(request, cancellationToken);
        }
        catch
        {
            await OnMainImageSetFailed(request, cancellationToken);
        }
    }

    private async Task OnMainImageSet(SetMainProductImageCommand request, CancellationToken cancellationToken)
    {
        await publisher.Publish(
            new ProductSnapshotMainImageSetEvent
            {
                CorrelationId = request.CorrelationId,
                SenderServiceName = serviceOptions.Value.Name,
                ProductId = request.ProductId,
            },
            cancellationToken);
        
        await productsRepository.SaveChangesAsync(cancellationToken);
    }
    
    private async Task OnMainImageSetFailed(SetMainProductImageCommand request, CancellationToken cancellationToken)
    {
        await publisher.PublishInIsolatedScopeAsync<OrdersDbContext>(
            new ProductSnapshotMainImageSetFailedEvent
            {
                CorrelationId = request.CorrelationId,
                SenderServiceName = serviceOptions.Value.Name,
                ProductId = request.ProductId,
            },
            cancellationToken);
    }
}