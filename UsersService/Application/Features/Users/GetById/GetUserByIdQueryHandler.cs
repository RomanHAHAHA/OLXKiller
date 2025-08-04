using Common.Domain.Models.Results;
using MediatR;
using UsersService.Domain.Entities;
using UsersService.Domain.Interfaces;

namespace UsersService.Application.Features.Users.GetById;

public class GetUserByIdQueryHandler(IUsersRepository usersRepository) : 
    IRequestHandler<GetUserByIdQuery, ApiResponse<User>>
{
    public async Task<ApiResponse<User>> Handle(
        GetUserByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetByIdAsync(request.UserId, cancellationToken);
            
        return user ?? ApiResponse<User>.NotFound("User not found");
    }
}