using System.Text.Json;
using Common.Application.Options;
using Common.Domain.Enums;
using Common.Domain.Interfaces;
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
    IOptions<ServiceOptions> serviceOptions,
    ICacheService<string> cacheService) : IRequestHandler<UpdateProductCommand, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productsRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return ApiResponse<Guid>.NotFound(nameof(Product));
        }

        var oldProductProperties = new OldProductProperties(product);
        
        if (!DtoHasChanges(product, request.ProductCreateDto))
        {
            //change frontend to receive API response (not only signalr response)
            return ApiResponse<Guid>.Conflict("Product properties equals to previous");
        }
        
        product.UpdateFromCreateDto(request.ProductCreateDto);
        
        await OnProductUpdated(request, cancellationToken);
        await productsRepository.SaveChangesAsync(cancellationToken);
        
        await cacheService.SetAsync(
            $"old-product:{oldProductProperties.Id}",
            JsonSerializer.Serialize(oldProductProperties),
            TimeSpan.FromMinutes(10),
            cancellationToken);

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
            new VerifyProductUpdatedEvent
            {
                CorrelationId = correlationId,
                SenderServiceName = serviceName,
                ProductId = request.ProductId,
                UserId = request.InitiatorUserId
            },
            context => context.Delay = TimeSpan.FromSeconds(30),
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

    private static bool DtoHasChanges(Product product, ProductCreateDto productCreateDto)
    {
        return product.Name != productCreateDto.Name || 
               product.Price != productCreateDto.Price;
    }
}