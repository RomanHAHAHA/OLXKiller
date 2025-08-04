using FluentValidation;

namespace ReviewsService.Application.Features.Reviews.Create;

public class ReviewCreateDtoValidator : AbstractValidator<ReviewCreateDto>
{
    public ReviewCreateDtoValidator()
    {
        RuleFor(p => p.ProductId)
            .NotEmpty().WithMessage("Name is required");
        
        RuleFor(p => p.Text)
            .NotEmpty().WithMessage("Text is required")
            .MaximumLength(255).WithMessage("Text must not exceed 255 characters")
            .MinimumLength(20).WithMessage("Text must have at least 20 characters");
        
        RuleFor(p => p.Rate)
            .NotEmpty().WithMessage("Rate is required")
            .GreaterThan(0).WithMessage("Rate must be greater than 0")
            .LessThanOrEqualTo(5).WithMessage("Price must be less or equal 5");
    }
}