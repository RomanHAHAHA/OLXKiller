using Common.Domain.Dtos;
using Common.Domain.Models.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersService.Domain.Entities;

namespace UsersService.Application.Features.Users.GetPagedList;

[Route("/api/users")]
[ApiController]
public class GetPagedUsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<PagedList<User>> GetPagedUsersListAsync(
        [FromQuery] UsersFilter usersFilter,
        [FromQuery] SortParams sortParams,
        [FromQuery] PageParams pageParams,
        CancellationToken cancellationToken)
    {
        var query = new GetPagedUsersListQuery(
            usersFilter,
            sortParams,
            pageParams);
        
        return await mediator.Send(query, cancellationToken);
    }
}