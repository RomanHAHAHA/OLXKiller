using MediatR;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.Products.Commands.UpdateProductRating;

public class UpdateProductRatingCommandHandler(
    IProductsRepository productsRepository,
    ILogger<UpdateProductRatingCommandHandler> logger) : IRequestHandler<UpdateProductRatingCommand>
{
    public async Task Handle(UpdateProductRatingCommand request, CancellationToken cancellationToken)
    {
        var product = await productsRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            logger.LogInformation($"Product with id {request.ProductId} not found");
            return;
        }

        product.AverageRating = request.AverageRating;
        
        var updated = await productsRepository.SaveChangesAsync(cancellationToken);
        var message = updated ? 
            $"Product {request.ProductId} rating updated!" : 
            $"Failed to update product with {request.ProductId}!";
        
        logger.LogInformation(message);
    }
}