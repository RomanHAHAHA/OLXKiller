using ChatsService.Domain.Entities;
using ChatsService.Infrastructure.Persistence;
using Common.Domain.Models.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatsService.Application.Features.Chats.Create;

public class CreateChatCommandHandler(
    ChatsDbContext dbContext) : IRequestHandler<CreateChatCommand, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(
        CreateChatCommand request, 
        CancellationToken cancellationToken)
    {
        var currentUser = await dbContext.UserSnapshots
            .FirstOrDefaultAsync(u => u.Id == request.CurrentUserId, cancellationToken);
        
        if (currentUser is null)
        {
            return ApiResponse<Guid>.NotFound("User");
        }

        var otherUser = await dbContext.UserSnapshots
            .FirstOrDefaultAsync(u => u.Id == request.OtherUSerId, cancellationToken);
        
        if (otherUser is null)
        {
            return ApiResponse<Guid>.NotFound("User");
        }

        var existingChat = await ChatExistsAsync(request, cancellationToken);

        if (existingChat is not null)
        {
            return ApiResponse<Guid>.Conflict("Chat between these users already exists");
        }
        
        var chat = new Chat();
        chat.Users.AddRange(currentUser, otherUser);
        
        await dbContext.Chats.AddAsync(chat, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return ApiResponse<Guid>.Ok(chat.Id);
    }

    private async Task<Chat?> ChatExistsAsync(
        CreateChatCommand request,
        CancellationToken cancellationToken)
    {
        return await dbContext.Chats
            .AsNoTracking()
            .Include(c => c.Users) 
            .FirstOrDefaultAsync(c => 
                    c.Users.Any(u => u.Id == request.CurrentUserId) && 
                    c.Users.Any(u => u.Id == request.OtherUSerId), 
                cancellationToken);
    }
}