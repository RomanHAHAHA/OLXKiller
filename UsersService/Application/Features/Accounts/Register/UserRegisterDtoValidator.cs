using FluentValidation;

namespace UsersService.Application.Features.Accounts.Register;

public class UserRegisterDtoValidator : AbstractValidator<UserRegisterDto>
{
    public UserRegisterDtoValidator()
    {
        RuleFor(registerUserDto => registerUserDto.NickName)
            .NotEmpty().WithMessage("Nickname is required")
            .MaximumLength(50).WithMessage("Nickname cannot exceed 50 symbols")
            .MinimumLength(3).WithMessage("Nickname must be at least 3 symbols long");

        RuleFor(registerUserDto => registerUserDto.Email)
            .NotEmpty().WithMessage("Email is required")
            .MaximumLength(50).WithMessage("Email cannot exceed 50 symbols")
            .MinimumLength(5).WithMessage("Email must be at least 5 symbols long")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(registerUserDto => registerUserDto.Password)
            .NotEmpty().WithMessage("Password is required")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 symbols")
            .MinimumLength(10).WithMessage("Password must be at least 10 symbols long");

        RuleFor(registerUserDto => registerUserDto.PasswordConfirm)
            .NotEmpty().WithMessage("Password confirmation is required")
            .Equal(registerUserDto => registerUserDto.Password).WithMessage("Passwords must match");

        RuleFor(registerUserDto => registerUserDto.ConnectionId)
            .NotEmpty().WithMessage("ERROR");
    }
}