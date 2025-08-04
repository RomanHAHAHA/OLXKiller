using Common.Domain.Models.Results;
using MediatR;
using UsersService.Domain.Entities;
using UsersService.Domain.Interfaces;

namespace UsersService.Application.Features.Users.GetPagedList;

public class GetPagedUsersListQueryHandler(IUsersRepository usersRepository) : 
    IRequestHandler<GetPagedUsersListQuery, PagedList<User>>
{
    public async Task<PagedList<User>> Handle(
        GetPagedUsersListQuery request, 
        CancellationToken cancellationToken)
    {
        return await usersRepository.GetPagedList(
            request.UsersFilter,
            request.SortParams,
            request.PageParams,
            cancellationToken); 
    }
}