using Common.Application.Options;
using Common.Domain.Interfaces;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.ProductImages.Create;

public class CreateImageCommandHandler(
    IProductsRepository productsRepository,
    IFileStorageService fileStorageService,
    IOptions<ProductImagesOptions> productImagesOptions,
    IOptions<ServiceOptions> serviceOptions,
    IPublishEndpoint publishEndpoint) : IRequestHandler<AddImagesCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(AddImagesCommand request, CancellationToken cancellationToken)
    {
        var product = await productsRepository.GetByIdWithImagesAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return ApiResponse.NotFound(nameof(Product));
        }

        var images = new List<ProductImage>();

        for (var i = 0; i < request.Images.Count; i++)
        {
            var result = await fileStorageService.SaveFileAsync(
                request.Images[i], 
                productImagesOptions.Value.Path, 
                cancellationToken);

            if (result.IsFailure)
            {
                return ApiResponse.BadRequest(result.Error);
            }

            var isMain = i == 0 && product.Images.Count == 0;
            var image = new ProductImage
            {
                ProductId = product.Id,
                ImagePath = result.Value,
                IsMain = isMain
            };

            images.Add(image);
            
            if (isMain)
            {
                await OnMainImageSet(image, cancellationToken);
            }
        }
        
        product.Images.AddRange(images);
        await productsRepository.SaveChangesAsync(cancellationToken);

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
                ImagePath = image.ImagePath,
            }, 
            cancellationToken);
    }
}