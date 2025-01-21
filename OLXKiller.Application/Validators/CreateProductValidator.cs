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
            .WithMessage("The product name is required.")
            .MinimumLength(MIN_NAME_LENGTH)
            .WithMessage($"The name must contain at least {MIN_NAME_LENGTH} symbols.")
            .MaximumLength(MAX_NAME_LENGTH)
            .WithMessage($"The name must not exceed {MAX_NAME_LENGTH} symbols.");

        RuleFor(p => p.Description)
            .NotEmpty()
            .WithMessage("The product description is required.")
            .MinimumLength(MIN_DESCRIPTION_LENGTH)
            .WithMessage($"The description must contain at least {MIN_DESCRIPTION_LENGTH} symbols.")
            .MaximumLength(MAX_DESCRIPTION_LENGTH)
            .WithMessage($"The description must not exceed {MAX_DESCRIPTION_LENGTH} symbols.");

        RuleFor(p => p.Price)
            .InclusiveBetween(MIN_PRICE_VALUE, MAX_PRICE_VALUE)
            .WithMessage($"The price must be between {MIN_PRICE_VALUE} and {MAX_PRICE_VALUE}.");

        RuleFor(p => p.Amount)
            .InclusiveBetween(MIN_AMOUNT_VALUE, MAX_AMOUNT_VALUE)
            .WithMessage($"The quantity must be between {MIN_AMOUNT_VALUE} and {MAX_AMOUNT_VALUE}.");
    }
}
