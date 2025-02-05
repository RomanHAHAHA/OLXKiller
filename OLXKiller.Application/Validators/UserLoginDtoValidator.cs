using FluentValidation;
using Microsoft.Extensions.Options;
using OLXKiller.Application.Dtos.User;
using OLXKiller.Application.Options;

namespace OLXKiller.Application.Validators;

public sealed class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
{
    private readonly UserValidationSettings _options;

    public UserLoginDtoValidator(IOptions<UserValidationSettings> options)
    {
        _options = options.Value;

        RuleFor(loginUserDto => loginUserDto.Email)
            .NotEmpty()
                .WithMessage("Email is required")
            .MaximumLength(_options.MaxEmailLength)
                .WithMessage($"Email cannot exceed {_options.MaxEmailLength} symbols")
            .MinimumLength(_options.MinEmailLength)
                .WithMessage($"Email must be at least {_options.MinEmailLength} symbols long")
            .EmailAddress()
                .WithMessage("Invalid email format");

        RuleFor(loginUserDto => loginUserDto.Password)
            .NotEmpty()
                .WithMessage("Password is required")
            .MaximumLength(_options.MaxPasswordLength)
                .WithMessage($"Password cannot exceed {_options.MaxPasswordLength} symbols")
            .MinimumLength(_options.MinPasswordLength)
                .WithMessage($"Password must be at least {_options.MinPasswordLength} symbols long");   
    }
}
