using ChatsService.Domain.Entities;
using ChatsService.Infrastructure.Persistence;
using Common.Domain.Models.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatsService.Application.Features.Users.Mute;

public class MuteUserCommandHandler(ChatsDbContext dbContext) : IRequestHandler<MuteUserCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(MuteUserCommand request, CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == request.UserToMuteId)
        {
            return ApiResponse.BadRequest("Cannot mute yourself");
        }

        if (!await IsUserExistAsync(request.CurrentUserId, cancellationToken))
        {
            return ApiResponse.NotFound("Current user");
        }

        if (!await IsUserExistAsync(request.UserToMuteId, cancellationToken))
        {
            return ApiResponse.NotFound("User to mute");
        }

        if (await IsUserAlreadyMuted(request, cancellationToken))
        {
            return ApiResponse.Conflict("User already muted");
        }

        var userMute = new UserMute
        {
            MutingUserId = request.CurrentUserId,
            MutedUserId = request.UserToMuteId,
        };
        
        dbContext.UserMutes.Add(userMute);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return ApiResponse.Ok();
    }

    private async Task<bool> IsUserExistAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.UserSnapshots.AnyAsync(u => u.Id == userId, cancellationToken);
    }
    
    private async Task<bool> IsUserAlreadyMuted(MuteUserCommand request, CancellationToken cancellationToken)
    {
        return await dbContext.UserMutes
            .AnyAsync(um => 
                um.MutingUserId == request.CurrentUserId && 
                um.MutedUserId == request.UserToMuteId, cancellationToken);
    } 
}