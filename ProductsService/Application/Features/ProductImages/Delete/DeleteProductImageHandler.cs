using Common.Application.Options;
using Common.Domain.Interfaces;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductsService.Application.Features.ProductImages.Create;
using ProductsService.Domain.Entities;
using ProductsService.Infrastructure.Persistence;

namespace ProductsService.Application.Features.ProductImages.Delete;

public class DeleteProductImageHandler(
    ProductsDbContext dbContext,
    IFileStorageService fileStorageService,
    IPublishEndpoint publishEndpoint,
    IOptions<ServiceOptions> serviceOptions,
    IOptions<ProductImagesOptions> imagesOptions) : IRequestHandler<DeleteProductImage, ApiResponse>
{
    public async Task<ApiResponse> Handle(DeleteProductImage request, CancellationToken cancellationToken)
    {
        var image = await dbContext.ProductImages
            .FirstOrDefaultAsync(i => i.Id == request.ImageId, cancellationToken);

        if (image is null)
        {
            return ApiResponse.NotFound("Product image");
        }

        dbContext.ProductImages.Remove(image);

        var imagePath = Path.Combine(imagesOptions.Value.Path, image.ImagePath);
        var deleteFileResult = await fileStorageService.DeleteFileAsync(imagePath, cancellationToken);

        if (deleteFileResult.IsFailure)
        {
            return ApiResponse.InternalServerError("Failed to delete image from file system");
        }

        if (image.IsMain)
        {
            await SetNewMainImageAsync(image, cancellationToken);
        }
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return ApiResponse.Ok();
    }
    
    private async Task SetNewMainImageAsync(ProductImage pastMainImage, CancellationToken cancellationToken)
    {
        var newMainImage = await dbContext.ProductImages
            .FirstOrDefaultAsync(i => 
                    i.ProductId == pastMainImage.ProductId && 
                    i.Id != pastMainImage.Id,
                cancellationToken);

        if (newMainImage is null)
        {
            await OnMainImageSet(pastMainImage.ProductId, string.Empty, cancellationToken);
            return;
        }

        newMainImage.IsMain = true;
        
        await OnMainImageSet(newMainImage.ProductId, newMainImage.ImagePath, cancellationToken);
    }
    
    private async Task OnMainImageSet(
        Guid productId,
        string imageName, 
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            new ProductMainImageSetEvent
            {
                CorrelationId = Guid.NewGuid(),
                SenderServiceName = serviceOptions.Value.Name,
                ProductId = productId,
                ImagePath = imageName
            }, 
            cancellationToken);
    }
}