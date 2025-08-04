using MediatR;
using UsersService.Domain.Interfaces;

namespace UsersService.Application.Features.Users.SetLoginTime;

public class SetUserLogInTimeCommandHandler(
    IUsersRepository usersRepository) : IRequestHandler<SetUserLogInTimeCommand>
{
    public async Task Handle(SetUserLogInTimeCommand request, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        user!.LastLogIn = request.LoggedInTime;
        
        await usersRepository.SaveChangesAsync(cancellationToken);
    }
}