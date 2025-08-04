using CartsService.Domain.Interfaces;
using MediatR;

namespace CartsService.Application.Features.Products.Delete;

public class DeleteProductCommandHandler(
    IProductsRepository productsRepository,
    ILogger<DeleteProductCommandHandler> logger) : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productsRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            logger.LogInformation($"Product with id: {request.ProductId} was not found");
            return;
        }
        
        productsRepository.Delete(product);
        var deleted  = await productsRepository.SaveChangesAsync(cancellationToken);
        
        var message = deleted ? 
            $"Failed to delete product with id: {request.ProductId}" : 
            $"Deleted product with id: {request.ProductId}";
        
        logger.LogInformation(message);
    }
}