using FluentValidation;

namespace UsersService.Application.Features.Users.UpdatePassword;

public class UpdatePasswordDtoValidator : AbstractValidator<UpdatePasswordDto>
{
    public UpdatePasswordDtoValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty()
            .WithMessage("Old password is required.")
            .MinimumLength(10)
            .WithMessage("Old password must be at least 10 characters long.")
            .MaximumLength(100)
            .WithMessage("Old password must not exceed 100 characters.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required.")
            .MinimumLength(10)
            .WithMessage("New password must be at least 10 characters long.")
            .MaximumLength(100)
            .WithMessage("New password must not exceed 100 characters.")
            .NotEqual(x => x.OldPassword)
            .WithMessage("New password must be different from the old password.");

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword)
            .WithMessage("Password confirmation does not match the new password.");
    }
}