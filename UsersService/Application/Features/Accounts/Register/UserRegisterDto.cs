namespace UsersService.Application.Features.Accounts.Register;

public record UserRegisterDto(
    string NickName,
    string Email,
    string Password,
    string PasswordConfirm,
    string ConnectionId);