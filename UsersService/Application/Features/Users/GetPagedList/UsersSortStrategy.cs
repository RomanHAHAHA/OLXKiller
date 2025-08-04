using System.Linq.Expressions;
using Common.Domain.Interfaces;
using UsersService.Domain.Entities;

namespace UsersService.Application.Features.Users.GetPagedList;

public class UsersSortStrategy : ISortStrategy<User>
{
    public Expression<Func<User, object>> GetKeySelector(string? orderBy)
    {
        return orderBy switch
        {
            nameof(User.NickName) => u => u.NickName,
            nameof(User.CreatedAt) => u => u.CreatedAt,
            nameof(User.RoleId) => u => u.RoleId,
            _ => p => p.Id
        };
    }
}