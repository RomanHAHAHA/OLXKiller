namespace UsersService.Application.Features.Users.GetPagedList;

public record UsersFilter(
    string? NickName,
    string? Email,
    int? RoleId,
    bool? IsEmailConfirmed,
    DateTime? StartRegisterDate,
    DateTime? EndRegisterDate);