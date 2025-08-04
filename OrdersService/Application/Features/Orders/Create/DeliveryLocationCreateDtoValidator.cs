using FluentValidation;
using OrdersService.Domain.Dtos;

namespace OrdersService.Application.Features.Orders.Create;

public class DeliveryLocationCreateDtoValidator : AbstractValidator<DeliveryLocationCreateDto>
{
    public DeliveryLocationCreateDtoValidator()
    {
        RuleFor(dl => dl.Region)
            .NotEmpty()
            .WithMessage("Please provide a valid region name");
        
        RuleFor(dl => dl.City)
            .NotEmpty()
            .WithMessage("Please provide a valid city name");
        
        RuleFor(dl => dl.Warehouse)
            .NotEmpty()
            .WithMessage("Please provide a valid warehouse name");
    }
}