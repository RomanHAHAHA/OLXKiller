using Common.Domain.Models.Results;
using MediatR;
using UsersService.Domain.Entities;
using UsersService.Domain.Interfaces;

namespace UsersService.Application.Features.Users.MarkEmailConfirmed;

public class MarkEmailAsConfirmedCommandHandler(IUsersRepository usersRepository) : 
    IRequestHandler<MarkEmailAsConfirmedCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(MarkEmailAsConfirmedCommand request, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            return ApiResponse.NotFound(nameof(User));
        }

        if (user.EmailConfirmed)
        {
            return ApiResponse.Ok();
        }
        
        user.EmailConfirmed = true;
        
        var updated = await usersRepository.SaveChangesAsync(cancellationToken);

        return updated ? 
            ApiResponse.Ok() :
            ApiResponse.InternalServerError("Failed to update user");
    }
}