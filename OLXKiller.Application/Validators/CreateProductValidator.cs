using FluentValidation;
using Microsoft.Extensions.Options;
using OLXKiller.Application.Dtos.Product;
using OLXKiller.Application.Options;

namespace OLXKiller.Application.Validators;

public class CreateProductValidator : AbstractValidator<ProductCreateDto>
{
    private readonly ProductValidationSettings _options;

    public CreateProductValidator(IOptions<ProductValidationSettings> options)
    {
        _options = options.Value;

        RuleFor(p => p.Name)
            .NotEmpty()
                .WithMessage("Name is required.")
            .MinimumLength(_options.MinNameLength)
                .WithMessage($"The name must contain at least {_options.MinNameLength} symbols.")
            .MaximumLength(_options.MaxNameLength)
                .WithMessage($"The name must not exceed {_options.MaxNameLength} symbols.");

        RuleFor(p => p.Description)
            .NotEmpty()
                .WithMessage("Description is required.")
            .MinimumLength(_options.MinDescriptionLength)
                .WithMessage($"The description must contain at least {_options.MinDescriptionLength} symbols.")
            .MaximumLength(_options.MaxNameLength)
                .WithMessage($"The description must not exceed {_options.MaxDescriptionLength} symbols.");

        RuleFor(p => p.Price)
            .NotEmpty()
                .WithMessage("Price is required.")
            .GreaterThan(_options.MinPriceValue - 1)
                .WithMessage($"Price must be greater than {_options.MinPriceValue - 1}")
            .LessThan(_options.MaxPriceValue)
                .WithMessage($"It`s too big value.");

        RuleFor(p => p.Amount)
            .NotEmpty()
                .WithMessage("Amount is required.")
            .GreaterThan(_options.MinAmountValue - 1)
                .WithMessage($"Amount must be greater than {_options.MinAmountValue - 1}")
            .LessThan(_options.MaxAmountValue)
                .WithMessage($"It`s too big value.");
    }
}
