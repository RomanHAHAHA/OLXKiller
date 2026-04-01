using FluentValidation;
using ProductsService.Application.Features.Categories.Commands.Update;

namespace ProductsService.Application.Common.Validators;

public class CategoryUpdateDtoValidator : AbstractValidator<CategoryUpdateDto>
{
    public CategoryUpdateDtoValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters")
            .MinimumLength(3).WithMessage("Name must have at least 3 characters");
        
        RuleFor(p => p.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}