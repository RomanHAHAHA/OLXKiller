using MediatR;

namespace OrdersService.Application.Features.Orders.Confirm;

public record ConfirmOrderProcessingCommand(Guid OrderId, Guid UserId) : IRequest;