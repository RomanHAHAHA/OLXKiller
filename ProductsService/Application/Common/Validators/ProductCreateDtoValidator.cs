using FluentValidation;
using ProductsService.Application.Common.Dtos;

namespace ProductsService.Application.Common.Validators;

public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateDtoValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters")
            .MinimumLength(3).WithMessage("Name must have at least 3 characters");
        
        RuleFor(p => p.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .MinimumLength(20).WithMessage("Description must have at least 20 characters");
        
        RuleFor(p => p.Price)
            .NotEmpty().WithMessage("Price is required")
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .LessThan(1_00_000).WithMessage("Price must be less than 100000");
        
        RuleFor(p => p.StockQuantity)
            .NotEmpty().WithMessage("StockQuantity is required")
            .GreaterThan(0).WithMessage("StockQuantity must be greater than 0")
            .LessThan(1_000_000).WithMessage("StockQuantity must be less than 1000000");
    }
}