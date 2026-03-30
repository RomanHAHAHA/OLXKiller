namespace UsersService.Application.Features.Accounts.UpdatePassword;

public record UpdatePasswordDto(
    string OldPassword,
    string NewPassword,
    string ConfirmNewPassword);