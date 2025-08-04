using FluentValidation;

namespace UsersService.Application.Features.Accounts.Login;

public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginDtoValidator()
    {
        RuleFor(registerUserDto => registerUserDto.Email)
            .NotEmpty().WithMessage("Email is required")
            .MaximumLength(50).WithMessage("Email cannot exceed 50 symbols")
            .MinimumLength(5).WithMessage("Email must be at least 5 symbols long")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(registerUserDto => registerUserDto.Password)
            .NotEmpty().WithMessage("Password is required")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 symbols")
            .MinimumLength(10).WithMessage("Password must be at least 10 symbols long");
    }
}