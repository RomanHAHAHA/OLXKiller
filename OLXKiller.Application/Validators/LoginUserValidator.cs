using FluentValidation;
using OLXKiller.Application.Dtos.User;
using static OLXKiller.Domain.Entities.UserEntity;

namespace OLXKiller.Application.Validators;

public class LoginUserValidator : AbstractValidator<LoginUserDto>
{
    public LoginUserValidator()
    {
        RuleFor(loginUserDto => loginUserDto.Email)
            .NotEmpty()
            .WithMessage("Email is required")

            .MaximumLength(MAX_EMAIL_LENGTH)
            .WithMessage($"Email cannot exceed {MAX_EMAIL_LENGTH} symbols")

            .MinimumLength(MIN_EMAIL_LENGTH)
            .WithMessage($"Email must be at least {MIN_EMAIL_LENGTH} symbols long")

            .EmailAddress()
            .WithMessage("Invalid email format");

        RuleFor(loginUserDto => loginUserDto.Password)
            .NotEmpty()
            .WithMessage("Password is required")

            .MaximumLength(MAX_PASSWORD_LENGTH)
            .WithMessage($"Password cannot exceed {MAX_PASSWORD_LENGTH} symbols")

            .MinimumLength(MIN_PASSWORD_LENGTH)
            .WithMessage($"Password must be at least {MIN_PASSWORD_LENGTH} symbols long");
    }
}
