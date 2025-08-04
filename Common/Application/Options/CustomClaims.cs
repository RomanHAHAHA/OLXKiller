namespace Common.Application.Options;

public class CustomClaims
{
    public static string UserId => nameof(UserId);

    public static string NickName => nameof(NickName);

    public static string AvatarImageName { get; set; } = nameof(AvatarImageName);
    
    public static string Role => nameof(Role);
    
    public static string Permissions => nameof(Permissions);
}