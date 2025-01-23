using Microsoft.EntityFrameworkCore;
using OLXKiller.Domain.Abstractions.Repositories;
using OLXKiller.Domain.Entities;
using OLXKiller.Persistence.Contexts;

namespace OLXKiller.Persistence.Repositories;

public class UsersRepository(AppDbContext appDbContext) :
    Repository<UserEntity>(appDbContext),
    IUsersRepository
{
    public async Task<UserEntity?> GetByEmail(string email)
        => await _appDbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);

    public async Task<UserEntity?> GetByIdWithAvatar(Guid userId)
        => await _appDbContext.Users
            .Include(u => u.Avatar)
            .FirstOrDefaultAsync(u => u.Id == userId);

    public override Task RemoveAsync(UserEntity entity)
    {
        var likesToRemove = _appDbContext.Likes
          .Where(like => like.UserId == entity.Id);

        _appDbContext.Likes.RemoveRange(likesToRemove);

        return base.RemoveAsync(entity);
    }
}
