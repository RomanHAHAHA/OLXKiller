namespace OLXKiller.Application.Dtos.User;

public class LoginedUserViewDto
{
    public string NickName { get; set; } = string.Empty;

    public string Avatar64String { get; set; } = string.Empty;

    public LoginedUserViewDto(string nickName, string avatarBase64String)
    {
        NickName = nickName;
        Avatar64String = avatarBase64String;
    }
}
