using Microsoft.EntityFrameworkCore;
using OLXKiller.Application.Abstractions;
using OLXKiller.Application.Dtos.User;
using OLXKiller.Domain.Abstractions.Models;
using OLXKiller.Domain.Abstractions.Providers;
using OLXKiller.Domain.Abstractions.Repositories;
using OLXKiller.Domain.Models;
using System.Net;

namespace OLXKiller.Application.Services;

public class AccountsService(
    IUsersRepository _usersRepository,
    IPasswordHasher _passwordHasher,
    IJwtProvider _jwtProvider) : IAccountsService
{
    public async Task<IBaseResponse<string>> Login(UserLoginDto loginUserDto)
    {
        var user = await _usersRepository.GetByEmailAsync(loginUserDto.Email);

        if (user is null)
        {
            return new BaseResponse<string>
                (HttpStatusCode.NotFound,
                "User with such email not found");
        }

        if (!_passwordHasher.Verify(loginUserDto.Password, user.HashedPassword))
        {
            return new BaseResponse<string>(
                HttpStatusCode.Unauthorized,
                "Incorrect password");
        }

        var token = await _jwtProvider.GenerateTokenAsync(user);

        return new BaseResponse<string>(
            HttpStatusCode.OK,
            data: token);
    }

    public async Task<IBaseResponse> Register(UserRegistrationDto registerUserDto)
    {
        var userToRegister = registerUserDto.AsUserEntity();

        userToRegister.HashedPassword = _passwordHasher
            .HashPassword(registerUserDto.Password);

        try
        {
            await _usersRepository.CreateAsync(userToRegister);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is not null
            && ex.InnerException.Message.Contains("unique index"))
        {
            return new BaseResponse(
                HttpStatusCode.Conflict,
                "User with this email already exists");
        }

        return new BaseResponse(HttpStatusCode.OK);
    }
}
