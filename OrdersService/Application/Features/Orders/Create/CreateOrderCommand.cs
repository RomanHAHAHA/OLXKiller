using Common.Domain.Dtos;
using Common.Domain.Models.Results;
using MediatR;
using OrdersService.Domain.Dtos;

namespace OrdersService.Application.Features.Orders.Create;

public record CreateOrderCommand(
    Guid UserId,
    DeliveryLocationCreateDto DeliveryLocationDto,
    List<CartItemDto> CartItems) : IRequest<ApiResponse>;