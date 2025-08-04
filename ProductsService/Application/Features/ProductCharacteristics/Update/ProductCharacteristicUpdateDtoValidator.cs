using FluentValidation;

namespace ProductsService.Application.Features.ProductCharacteristics.Update;

public class ProductCharacteristicUpdateDtoValidator : AbstractValidator<ProductCharacteristicUpdateDto>
{
    public ProductCharacteristicUpdateDtoValidator()
    {
        RuleFor(x => x.NewName)
            .NotEmpty().WithMessage("New name is required")
            .MaximumLength(255).WithMessage("New name is too long");

        RuleFor(x => x.OldName)
            .NotEmpty().WithMessage("Old name is required")
            .NotEqual(x => x.NewName).WithMessage("Old and new name must be different");
        
        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Value is required")
            .MaximumLength(255).WithMessage("Value is too long");
    }
}