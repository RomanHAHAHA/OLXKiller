using Common.Application.Options;
using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.Products.Commands.CheckStockQuantity;

public class CheckProductStockQuantityCommandHandler(
    IProductsRepository productsRepository,
    ILogger<CheckProductStockQuantityCommandHandler> logger,
    IPublishEndpoint publisher,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<CheckProductStockQuantityCommand>
{
    public async Task Handle(CheckProductStockQuantityCommand request, CancellationToken cancellationToken)
    {
        var product = await productsRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            logger.LogInformation($"Product with id {request.ProductId} was not found");
            return;
        }

        if (product.StockQuantity < request.RequestedQuantity)
        {
            await OnProductStockExceeded(request.UserId, product.StockQuantity, cancellationToken);
            await productsRepository.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task OnProductStockExceeded(
        Guid userId,
        int stockQuantity, 
        CancellationToken cancellationToken)
    {
        await publisher.Publish(
            new ProductStockExceededEvent
            {
                CorrelationId = Guid.NewGuid(),
                SenderServiceName = serviceOptions.Value.Name,
                UserId = userId,
                StockQuantity = stockQuantity,
            },
            cancellationToken);
    }
}