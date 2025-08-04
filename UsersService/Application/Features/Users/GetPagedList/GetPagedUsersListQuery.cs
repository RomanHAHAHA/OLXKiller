using Common.Domain.Dtos;
using Common.Domain.Models.Results;
using MediatR;
using UsersService.Domain.Entities;

namespace UsersService.Application.Features.Users.GetPagedList;

public record GetPagedUsersListQuery(
    UsersFilter UsersFilter,
    SortParams SortParams,
    PageParams PageParams) : IRequest<PagedList<User>>;