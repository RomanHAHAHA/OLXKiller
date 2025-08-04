using Common.Application.Options;
using Common.Domain.Enums;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.Product;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using ProductsService.Application.Common.Dtos;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Extensions;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.Products.Commands.Update;

public class UpdateProductCommandHandler(
    IProductsRepository productsRepository,
    IPublishEndpoint publishEndpoint,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<UpdateProductCommand, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productsRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return ApiResponse<Guid>.NotFound(nameof(Product));
        }
        
        if (IsSnapshotPropertiesUpdated(product, request.ProductCreateDto))
        {
            await OnProductUpdated(request, cancellationToken);
        }
        
        product.UpdateFromCreateDto(request.ProductCreateDto);
        
        await productsRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.Ok(product.Id);
    }

    private async Task OnProductUpdated(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();
        var serviceName = serviceOptions.Value.Name;
        
        await publishEndpoint.Publish(
            new SystemActionEvent
            {
                CorrelationId = correlationId,
                SenderServiceName = serviceName,
                UserId = request.InitiatorUserId,
                ActionType = ActionType.Update,
                Message = $"Product {request.ProductId} updated"
            },
            cancellationToken);
        
        await publishEndpoint.Publish(
            new ProductUpdatedEvent
            {
                CorrelationId = correlationId,
                SenderServiceName = serviceName,
                ProductId = request.ProductId,
                UserId = request.InitiatorUserId,
                Name = request.ProductCreateDto.Name,
                Price = request.ProductCreateDto.Price!.Value,
                StockQuantity = request.ProductCreateDto.StockQuantity!.Value,
            }, 
            cancellationToken);
    }

    private static bool IsSnapshotPropertiesUpdated(Product product, ProductCreateDto productCreateDto)
    {
        return product.Name != productCreateDto.Name || 
               product.Price != productCreateDto.Price;
    }
}