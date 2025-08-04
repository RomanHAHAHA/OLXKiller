using Common.Domain.Extensions;
using Common.Domain.Interfaces;
using UsersService.Domain.Entities;

namespace UsersService.Application.Features.Users.GetPagedList;

public class UsersFilterStrategy : IFilterStrategy<User, UsersFilter>
{
    public IQueryable<User> Filter(IQueryable<User> query, UsersFilter filter)
    {
        return query
            .WhereIf(!string.IsNullOrWhiteSpace(filter.NickName), 
                u => u.NickName.StartsWith(filter.NickName!))
            
            .WhereIf(!string.IsNullOrWhiteSpace(filter.Email), 
                u => u.Email.StartsWith(filter.Email!))
            
            .WhereIf(filter.RoleId.HasValue,
                u => u.RoleId == filter.RoleId)
            
            .WhereIf(filter.StartRegisterDate.HasValue,
                u => u.CreatedAt >= filter.StartRegisterDate)
            
            .WhereIf(filter.EndRegisterDate.HasValue, 
                u => u.CreatedAt <= filter.EndRegisterDate)
            
            .WhereIf(filter.IsEmailConfirmed.HasValue, 
                u => u.EmailConfirmed == filter.IsEmailConfirmed);
    }
}