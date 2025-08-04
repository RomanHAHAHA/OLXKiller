using MediatR;
using ReviewsService.Domain.Interfaces;

namespace ReviewsService.Application.Features.Products.Delete;

public class DeleteProductCommandHandler(
    IProductsRepository productRepository,
    ILogger<DeleteProductCommandHandler> logger) : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            logger.LogInformation($"Product with id: {request.ProductId} was not found");
            return;
        }
        
        productRepository.Delete(product);
        var deleted  = await productRepository.SaveChangesAsync(cancellationToken);

        var message = deleted ? 
            $"Deleted product with id: {request.ProductId}" : 
            $"Failed to delete product with id: {request.ProductId}"; 
        
        logger.LogInformation(message);
    }
}