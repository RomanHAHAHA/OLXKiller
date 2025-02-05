using FluentValidation;
using Microsoft.Extensions.Options;
using OLXKiller.Application.Dtos.User;
using OLXKiller.Application.Options;

namespace OLXKiller.Application.Validators;

public class UserRegistrationDtoValidator : AbstractValidator<UserRegistrationDto>
{
    private readonly UserValidationSettings _options;

    public UserRegistrationDtoValidator(IOptions<UserValidationSettings> options)
    {
        _options = options.Value;

        RuleFor(registerUserDto => registerUserDto.NickName)
            .NotEmpty()
                .WithMessage("Nickname is required")
            .MaximumLength(_options.MaxNickNameLength)
                .WithMessage($"Nickname cannot exceed {_options.MaxNickNameLength} symbols")
            .MinimumLength(_options.MinNickNameLength)
                .WithMessage($"Nickname must be at least {_options.MinNickNameLength} symbols long");

        RuleFor(registerUserDto => registerUserDto.Email)
            .NotEmpty()
                .WithMessage("Email is required")
            .MaximumLength(_options.MaxEmailLength)
                .WithMessage($"Email cannot exceed {_options.MaxEmailLength} symbols")
            .MinimumLength(_options.MinEmailLength)
                .WithMessage($"Email must be at least {_options.MinEmailLength} symbols long")
            .EmailAddress()
                .WithMessage("Invalid email format"); 

        RuleFor(registerUserDto => registerUserDto.Password)
            .NotEmpty()
                .WithMessage("Password is required")
            .MaximumLength(_options.MaxPasswordLength)
                .WithMessage($"Password cannot exceed {_options.MaxPasswordLength} symbols")
            .MinimumLength(_options.MinPasswordLength)
                .WithMessage($"Password must be at least {_options.MinPasswordLength} symbols long");

        RuleFor(registerUserDto => registerUserDto.PasswordConfirm)
            .NotEmpty()
                .WithMessage("Password confirmation is required")
            .Equal(registerUserDto => registerUserDto.Password)
                .WithMessage("Passwords must match");
    }
}

