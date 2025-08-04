using Common.Application.Options;
using Common.Domain.Enums;
using Common.Domain.Interfaces;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using ProductsService.Domain.Dtos;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.Products.Queries.GetProductFullInfo;

public class GetProductInfoQueryHandler(
    IProductsRepository productsRepository,
    IPublishEndpoint publishEndpoint,
    IHttpUserContext httpContext,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<GetProductInfoQuery, ApiResponse<ProductInfoDto>>
{
    public async Task<ApiResponse<ProductInfoDto>> Handle(GetProductInfoQuery request, CancellationToken cancellationToken)
    {
        var product = await productsRepository
            .GetAllInfoByIdAsync(request.ProductId, cancellationToken);

        await OnProductRead(request.ProductId, product, cancellationToken);
        
        if (product is null)
        {
            return ApiResponse<ProductInfoDto>.NotFound(nameof(Product));
        }

        return new ProductInfoDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Rating = product.AverageRating,
            IsMine = !request.UserId.Equals(Guid.Empty) && product.UserId == request.UserId,
            Seller = new ProductSellerDto(product),
            Categories = product.Categories
                .Select(c => new ShortCategoryDto(c.Id, c.Name))
                .ToList(),
            Images = product.Images
                .OrderByDescending(i => i.IsMain)
                .Select(i => new ShortImageDto(i.Id, i.ImagePath))
                .ToList(),
            Characteristics = product.Characteristics
                .Select(pc => new ProductCharacteristicViewDto(pc.Name, pc.Value))
                .ToList()
        };
    }

    private async Task OnProductRead(
        Guid productId, 
        Product? product, 
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new SystemActionEvent
        {
            CorrelationId = Guid.NewGuid(),
            SenderServiceName = serviceOptions.Value.Name,
            UserId = httpContext.UserId,
            ActionType = ActionType.Read,
            Message = product is null ? $"Product {productId} not found" : $"Product {productId} read"
        }, cancellationToken);
        
        await productsRepository.SaveChangesAsync(cancellationToken);
    }
}