namespace OLXKiller.Domain.Entities;

public class UserAvatarEntity
{
    public Guid UserId { get; set; }

    public UserEntity? User { get; set; }

    public byte[] Data { get; set; } = [];

    private UserAvatarEntity() { }

    public UserAvatarEntity(Guid userId, byte[] data)
    {
        UserId = userId;
        Data = data;
    }
}
