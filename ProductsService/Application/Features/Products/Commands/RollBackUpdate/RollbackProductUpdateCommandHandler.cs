using System.Text.Json;
using Common.Application.Options;
using Common.Domain.Interfaces;
using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using ProductsService.Application.Features.Products.Commands.Update;
using ProductsService.Domain.Extensions;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.Products.Commands.RollBackUpdate;

public class RollbackProductUpdateCommandHandler(
    IProductsRepository productsRepository,
    ICacheService<string> cacheService,
    ILogger<RollbackProductUpdateCommandHandler> logger,
    IPublishEndpoint publisher,
    IOptions<ServiceOptions> options) : IRequestHandler<RollbackProductUpdateCommand>
{
    public async Task Handle(RollbackProductUpdateCommand request, CancellationToken cancellationToken)
    {
        var product = await productsRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            logger.LogError($"Unable to rollback product. Product {request.ProductId} not found.");
            return;
        }

        var oldProduct = await cacheService.GetAsync($"old-product:{request.ProductId}", cancellationToken);

        if (oldProduct is null)
        {
            logger.LogError($"Unable to rollback product. Product cache {request.ProductId} not found.");
            return;
        }
        
        var deserializedOldProduct = JsonSerializer.Deserialize<OldProductProperties>(oldProduct);

        if (deserializedOldProduct is null)
        {
            logger.LogError("Unable to rollback product. Deserialization failed.");
            return;
        }
        
        product.CopyProperties(deserializedOldProduct);
        await OnProductUpdateRollback(request, deserializedOldProduct, cancellationToken);
        await productsRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task OnProductUpdateRollback(
        RollbackProductUpdateCommand request,
        OldProductProperties oldProduct,
        CancellationToken cancellationToken)
    {
        await publisher.Publish(
            new ProductRolledBackEvent
            {
                CorrelationId = request.CorrelationId,
                SenderServiceName = options.Value.Name,
                Id = oldProduct.Id,
                Name = oldProduct.Name,
                Price = oldProduct.Price,
            }, 
            cancellationToken);
    }
}