using ChatsService.Infrastructure.Persistence;
using Common.Domain.Models.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatsService.Application.Features.Users.Unmute;

public class UnmuteUserCommandHandler(
    ChatsDbContext dbContext) : IRequestHandler<UnmuteUserCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(UnmuteUserCommand request, CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == request.UserToUnmuteId)
        {
            return ApiResponse.BadRequest("Cannot unmute yourself");    
        }
        
        var rowsAffected = await dbContext.UserMutes
            .Where(um => 
                um.MutingUserId == request.CurrentUserId && 
                um.MutedUserId == request.UserToUnmuteId)
            .ExecuteDeleteAsync(cancellationToken);

        return rowsAffected == 0 ? ApiResponse.NotFound("User to unmute") : ApiResponse.Ok();
    }
}