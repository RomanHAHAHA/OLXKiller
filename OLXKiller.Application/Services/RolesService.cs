using Microsoft.EntityFrameworkCore;
using OLXKiller.Application.Abstractions;
using OLXKiller.Application.Dtos.Role;
using OLXKiller.Domain.Abstractions.Models;
using OLXKiller.Domain.Abstractions.Repositories;
using OLXKiller.Domain.Models;
using System.Net;

namespace OLXKiller.Application.Services;

public class RolesService(
    IRolesRepository _rolesRepository,
    IUsersRepository _usersRepository) : IRolesService
{
    public async Task<List<RoleForSelectDto>> GetRolesForSelectAsync()
        => await _rolesRepository
            .GetAllAsync()
            .AsNoTracking()
            .Select(r => new RoleForSelectDto(r))
            .ToListAsync();

    public async Task<IBaseResponse> AssignRoleToUserAsync(Guid userId, int roleId)
    {
        var user = await _usersRepository.GetByIdAsync(userId);

        if (user is null)
            return new BaseResponse(
                HttpStatusCode.NotFound, "User was not found");

        var role = await _rolesRepository.GetByIdAsync(roleId);

        if (role is null)
            return new BaseResponse(
                HttpStatusCode.BadRequest, "Invalid role id received from client");

        user.Role = role;

        await _usersRepository.UpdateAsync(user);

        return new BaseResponse(HttpStatusCode.OK);
    }
}
