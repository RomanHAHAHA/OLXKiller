namespace OLXKiller.Application.Dtos.User;

public class CollectionUserDto
{
    public Guid Id { get; set; }

    public string NickName { get; set; } = string.Empty;

    public byte[] AvatarBytes { get; set; } = [];

    public string RoleName { get; set; } = string.Empty;
}
