using MediatR;

namespace OrdersService.Application.Features.Orders.Commands.Confirm;

public record ConfirmOrderProcessingCommand(Guid OrderId) : IRequest;