using FluentValidation;
using OLXKiller.Application.Dtos.User;
using static OLXKiller.Domain.Entities.UserEntity;

namespace OLXKiller.Application.Validators;

public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserValidator()
    {
        RuleFor(registerUserDto => registerUserDto.NickName)
            .NotEmpty()
            .WithMessage("Nickname is required")

            .MaximumLength(MAX_NICK_NAME_LENGTH)
            .WithMessage($"Nickname cannot exceed {MAX_NICK_NAME_LENGTH} symbols")

            .MinimumLength(MIN_NICK_NAME_LENGTH)
            .WithMessage($"Nickname must be at least {MIN_NICK_NAME_LENGTH} symbols long");

        RuleFor(registerUserDto => registerUserDto.Email)
            .NotEmpty()
            .WithMessage("Email is required")

            .MaximumLength(MAX_EMAIL_LENGTH)
            .WithMessage($"Email cannot exceed {MAX_EMAIL_LENGTH} symbols")

            .MinimumLength(MIN_EMAIL_LENGTH)
            .WithMessage($"Email must be at least {MIN_EMAIL_LENGTH} symbols long")
            
            .EmailAddress()
            .WithMessage("Invalid email format"); 

        RuleFor(registerUserDto => registerUserDto.Password)
            .NotEmpty()
            .WithMessage("Password is required")

            .MaximumLength(MAX_PASSWORD_LENGTH)
            .WithMessage($"Password cannot exceed {MAX_PASSWORD_LENGTH} symbols")

            .MinimumLength(MIN_PASSWORD_LENGTH)
            .WithMessage($"Password must be at least {MIN_PASSWORD_LENGTH} symbols long");

        RuleFor(registerUserDto => registerUserDto.PasswordConfirm)
            .NotEmpty()
            .WithMessage("Password confirmation is required")

            .Equal(registerUserDto => registerUserDto.Password)
            .WithMessage("Passwords must match"); 
    }
}

