using OLXKiller.Application.Dtos.User;
using OLXKiller.Domain.Abstractions.Models;

namespace OLXKiller.Application.Abstractions;

public interface IAccountsService
{
    Task<IBaseResponse<string>> Login(LoginUserDto loginUserDto);

    Task<IBaseResponse> Register(RegisterUserDto registerUserDto);
}
