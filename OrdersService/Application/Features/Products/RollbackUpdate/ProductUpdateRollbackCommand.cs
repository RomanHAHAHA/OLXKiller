using Common.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrdersService.Infrastructure.Persistence;

namespace OrdersService.Application.Features.Products.RollbackUpdate;

public record ProductUpdateRollbackCommand(ProductSnapshot Snapshot) : IRequest;

public class ProductUpdateRollbackCommandHandler(
    OrdersDbContext dbContext,
    ILogger<ProductUpdateRollbackCommandHandler> logger) : IRequestHandler<ProductUpdateRollbackCommand>
{
    public async Task Handle(ProductUpdateRollbackCommand request, CancellationToken cancellationToken)
    {
        var product = await dbContext.Product
            .FirstOrDefaultAsync(p => p.Id == request.Snapshot.Id, cancellationToken);

        if (product is null)
        {
            logger.LogInformation($"Product {request.Snapshot.Id} not found!");
            return;
        }
        
        product.Name = request.Snapshot.Name;
        product.Price = request.Snapshot.Price;
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation($"Product {request.Snapshot.Id} update rolled back!");
    }
}
