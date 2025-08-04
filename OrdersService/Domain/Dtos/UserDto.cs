namespace OrdersService.Domain.Dtos;

public class UserDto
{
    public required Guid Id { get; init; }

    public required string NickName { get; init; }

    public required string AvatarName { get; init; }
}