using Common.Domain.Models.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReviewsService.Infrastructure.Persistence;

namespace ReviewsService.Application.Features.Reviews.HasReviewedProduct;

public class HasReviewedProductQueryHandler(
    ReviewsDbContext dbContext) : IRequestHandler<HasReviewedProductQuery, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(
        HasReviewedProductQuery request, 
        CancellationToken cancellationToken)
    {
        if (!await dbContext.UserSnapshots.AnyAsync(u => u.Id == request.UserId, cancellationToken))
        {
            return ApiResponse<bool>.NotFound("User");
        }
        
        if (!await dbContext.ProductSnapshots.AnyAsync(p => p.Id == request.ProductId, cancellationToken))
        {
            return ApiResponse<bool>.NotFound("Product");
        }
        
        var hasReviewedProduct = await dbContext.Reviews
            .AnyAsync(r => 
                    r.ProductId == request.ProductId && 
                    r.UserId == request.UserId,
                cancellationToken);

        return hasReviewedProduct;
    }
}