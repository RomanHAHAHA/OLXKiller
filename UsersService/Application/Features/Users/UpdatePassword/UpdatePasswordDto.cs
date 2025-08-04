namespace UsersService.Application.Features.Users.UpdatePassword;

public record UpdatePasswordDto(
    string OldPassword,
    string NewPassword,
    string ConfirmNewPassword);