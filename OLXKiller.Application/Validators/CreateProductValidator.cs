using FluentValidation;
using OLXKiller.Application.Dtos.Product;
using static OLXKiller.Domain.Entities.ProductEntity;

namespace OLXKiller.Application.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .WithMessage("Name is required.")

            .MinimumLength(MIN_NAME_LENGTH)
            .WithMessage($"The name must contain at least {MIN_NAME_LENGTH} symbols.")

            .MaximumLength(MAX_NAME_LENGTH)
            .WithMessage($"The name must not exceed {MAX_NAME_LENGTH} symbols.");

        RuleFor(p => p.Description)
            .NotEmpty()
            .WithMessage("Description is required.")

            .MinimumLength(MIN_DESCRIPTION_LENGTH)
            .WithMessage($"The description must contain at least {MIN_DESCRIPTION_LENGTH} symbols.")

            .MaximumLength(MAX_DESCRIPTION_LENGTH)
            .WithMessage($"The description must not exceed {MAX_DESCRIPTION_LENGTH} symbols.");

        RuleFor(p => p.Price)
            .NotEmpty()
            .WithMessage("Price is required.")

            .GreaterThan(MIN_PRICE_VALUE - 1)
            .WithMessage($"Price must be greater than {MIN_PRICE_VALUE - 1}")

            .LessThan(MAX_PRICE_VALUE)
            .WithMessage($"It`s too big value.");

        RuleFor(p => p.Amount)
            .NotEmpty()
            .WithMessage("Amount is required.")

            .GreaterThan(MIN_AMOUNT_VALUE - 1)
            .WithMessage($"Amount must be greater than {MIN_AMOUNT_VALUE - 1}")

            .LessThan(MAX_AMOUNT_VALUE)
            .WithMessage($"It`s too big value.");
    }
}
