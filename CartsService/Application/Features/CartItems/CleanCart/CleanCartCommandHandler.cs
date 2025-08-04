using CartsService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CartsService.Application.Features.CartItems.CleanCart;

public class CleanCartCommandHandler(CartsDbContext dbContext) : IRequestHandler<CleanCartCommand>
{
    public async Task Handle(CleanCartCommand request, CancellationToken cancellationToken)
    {
        var cartItems = await dbContext.CartItems
            .Where(ci => ci.UserId == request.UserId)
            .ToListAsync(cancellationToken);
        
        dbContext.RemoveRange(cartItems);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}