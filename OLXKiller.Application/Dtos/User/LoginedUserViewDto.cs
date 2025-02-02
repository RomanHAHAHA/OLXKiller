namespace OLXKiller.Application.Dtos.User;

public class LoginedUserViewDto
{
    public string NickName { get; set; } = string.Empty;

    public byte[] AvatarBytes { get; set; } = [];

    public LoginedUserViewDto(string nickName, byte[] avatarBase64String)
    {
        NickName = nickName;
        AvatarBytes = avatarBase64String;
    }
}
