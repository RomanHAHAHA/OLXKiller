namespace OLXKiller.Domain.Entities;

public class UserEntity
{   
    public Guid Id { get; set; }

    public string NickName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string HashedPassword { get; set; } = string.Empty;

    public UserAvatarEntity? Avatar { get; set; }

    public ICollection<ProductEntity> LikedProducts { get; set; } = [];

    public ICollection<CartItemEntity> CartItems { get; set; } = [];

    public int RoleId { get; set; }

    public RoleEntity? Role { get; set; }

    public UserEntity() { }
}
