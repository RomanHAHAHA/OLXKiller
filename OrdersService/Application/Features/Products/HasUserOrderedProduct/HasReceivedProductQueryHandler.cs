using Common.Domain.Entities;
using Common.Domain.Enums;
using Common.Domain.Models.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Infrastructure.Persistence;

namespace OrdersService.Application.Features.Products.HasUserOrderedProduct;

public class HasReceivedProductQueryHandler(
    OrdersDbContext dbContext) : IRequestHandler<HasReceivedProductQuery, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(HasReceivedProductQuery request, CancellationToken cancellationToken)
    {
        if (!await dbContext.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken))
        {
            return ApiResponse<bool>.NotFound(nameof(UserSnapshot));
        }
        
        if (!await dbContext.Product.AnyAsync(p => p.Id == request.ProductId, cancellationToken))
        {
            return ApiResponse<bool>.NotFound(nameof(ProductSnapshot));
        }
        
        return await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.UserId == request.UserId && o.CurrentStatus == OrderStatus.Received)
            .SelectMany(o => o.OrderItems)
            .AnyAsync(oi => oi.ProductId == request.ProductId, cancellationToken);
    }
}