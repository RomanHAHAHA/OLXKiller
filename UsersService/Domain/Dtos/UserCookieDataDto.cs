namespace UsersService.Domain.Dtos;

public class UserCookieDataDto
{
    public string UserId { get; set; } = string.Empty;

    public string NickName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string AvatarImageName { get; set; } = string.Empty;

    public List<string> Permissions { get; set; } = [];
}