namespace OLXKiller.Domain.Entities;

public class UserEntity
{
    public const int MAX_NICK_NAME_LENGTH = 100;
    public const int MIN_NICK_NAME_LENGTH = 5;
    public const int MAX_EMAIL_LENGTH = 200;
    public const int MIN_EMAIL_LENGTH = 15;
    public const int MAX_PASSWORD_LENGTH = 200;
    public const int MIN_PASSWORD_LENGTH = 10;

    public Guid Id { get; set; }

    public string NickName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string HashedPassword { get; set; } = string.Empty;

    public UserAvatarEntity? Avatar { get; set; }

    public IEnumerable<ProductEntity> ProductsForSale { get; set; } = [];

    public UserEntity() { }
}
