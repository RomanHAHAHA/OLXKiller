using FluentValidation;
using ProductsService.Application.Common.Dtos;

namespace ProductsService.Application.Common.Validators;

public class CategoryCreateDtoValidator : AbstractValidator<CategoryCreateDto>
{
    public CategoryCreateDtoValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters")
            .MinimumLength(3).WithMessage("Name must have at least 3 characters");
        
        RuleFor(p => p.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .MinimumLength(20).WithMessage("Description must have at least 20 characters");
    }
}