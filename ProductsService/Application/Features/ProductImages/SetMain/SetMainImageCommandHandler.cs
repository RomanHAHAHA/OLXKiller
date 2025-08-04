using Common.Application.Options;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.ProductImages.SetMain;

public class SetMainImageCommandHandler(
    IProductImagesRepository imagesRepository,
    IPublishEndpoint publishEndpoint,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<SetMainImageCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(SetMainImageCommand request, CancellationToken cancellationToken)
    {
        var newMainImage = await imagesRepository.GetByIdAsync(request.ImageId, cancellationToken);

        if (newMainImage is null)
        {
            return ApiResponse.NotFound("Image not found");
        }
        
        var productImages = await imagesRepository.GetProductImagesAsync(newMainImage.ProductId, cancellationToken);
        var pastMainImage = productImages.FirstOrDefault(i => i.IsMain && i.Id != request.ImageId);

        if (pastMainImage is null)
        {
            return ApiResponse.NotFound("Failed to find past main image");
        }
        
        await using var transaction = await imagesRepository.BeginTransactionAsync(cancellationToken: cancellationToken);

        pastMainImage.IsMain = false;
        newMainImage.IsMain = true;

        await OnMainImageSet(newMainImage, cancellationToken);
        
        await imagesRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
            
        return ApiResponse.Ok();
    }

    
    private async Task OnMainImageSet(ProductImage image, CancellationToken cancellationToken = default)
    {
        await publishEndpoint.Publish(
            new ProductMainImageSetEvent
            {
                CorrelationId = Guid.NewGuid(),
                SenderServiceName = serviceOptions.Value.Name,
                ProductId = image.ProductId,
                ImagePath = image.ImagePath
            }, 
            cancellationToken);
    }
}