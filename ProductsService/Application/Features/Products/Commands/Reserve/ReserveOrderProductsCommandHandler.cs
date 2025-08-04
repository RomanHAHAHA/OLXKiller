using System.Data;
using Common.Application.Options;
using Common.Domain.Dtos;
using Common.Infrastructure.Messaging.Events.Product;
using Common.Infrastructure.Messaging.Publishers;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Application.Features.Products.Commands.Reserve;

public class ReserveOrderProductsCommandHandler(
    IProductsRepository productsRepository,
    IPublishEndpoint publisher,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<ReserveOrderProductsCommand>
{
    public async Task Handle(ReserveOrderProductsCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await productsRepository
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        try
        {
            var productIds = request.CartItems.Select(ci => ci.Product.Id).ToList();
            var products = await productsRepository.GetProductsByIdsAsync(productIds, cancellationToken);
        
            if (products.Count != request.CartItems.Count)
            {
                await OnProductsReservationFailed(request, [], cancellationToken);
                return;
            }
        
            var outOfStock = GetOutOfStockProducts(products, request.CartItems);
            
            if (outOfStock.Count != 0)
            {
                await OnProductsReservationFailed(request, outOfStock, cancellationToken);
                return;
            }
        
            UpdateStock(products, request.CartItems);
            
            await OnProductsReserved(request, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            await OnProductsReservationFailed(request, [], cancellationToken);
        }
    }
    
    private static void UpdateStock(List<Product> products, List<CartItemDto> cartItems)
    {
        var requestedQuantities = cartItems.ToDictionary(ci => ci.Product.Id, ci => ci.Quantity);

        foreach (var product in products)
        {
            if (requestedQuantities.TryGetValue(product.Id, out var requestedQuantity))
            {
                product.StockQuantity -= requestedQuantity;
            }
        }
    }

    private static List<ProductStockInfo> GetOutOfStockProducts(List<Product> products, List<CartItemDto> cartItems)
    {
        var outOfStock = new List<ProductStockInfo>();
        var requestedQuantities = cartItems.ToDictionary(ci => ci.Product.Id, ci => ci.Quantity);

        foreach (var product in products)
        {
            if (!requestedQuantities.TryGetValue(product.Id, out var requestedQuantity) ||
                product.StockQuantity < requestedQuantity)
            {
                outOfStock.Add(new ProductStockInfo(product.Id, product.StockQuantity));
            }
        }

        return outOfStock;
    }
    
    private async Task OnProductsReservationFailed(
        ReserveOrderProductsCommand request,
        List<ProductStockInfo> productStockInfos,
        CancellationToken cancellationToken)
    {
        await publisher.PublishInIsolatedScopeAsync<ProductsDbContext>(
            new ProductsReservationFailedEvent
            {
                CorrelationId = Guid.NewGuid(),
                SenderServiceName = serviceOptions.Value.Name,
                OrderId = request.OrderId,
                UserId = request.UserId,
                ProductStockInfos = productStockInfos
            }, 
            cancellationToken);
    }

    private async Task OnProductsReserved(ReserveOrderProductsCommand request, CancellationToken cancellationToken)
    {
        await publisher.Publish(
            new ProductsReservedEvent
            {
                CorrelationId = Guid.NewGuid(),
                SenderServiceName = serviceOptions.Value.Name,
                OrderId = request.OrderId,
                UserId = request.UserId,
            }, 
            cancellationToken);
        
        await productsRepository.SaveChangesAsync(cancellationToken);
    }
}