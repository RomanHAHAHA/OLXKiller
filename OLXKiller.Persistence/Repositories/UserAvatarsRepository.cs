using OLXKiller.Persistence.Repositories;
using OLXKiller.Persistence.Contexts;
using OLXKiller.Domain.Entities;

namespace ProjectX.Infrastructure.Repositories;

public class UserAvatarsRepository(AppDbContext appDbContext) : 
    Repository<UserAvatarEntity>(appDbContext)
{
}
