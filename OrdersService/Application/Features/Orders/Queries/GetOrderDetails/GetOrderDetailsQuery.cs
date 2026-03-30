using Common.Domain.Models.Results;
using MediatR;

namespace OrdersService.Application.Features.Orders.Queries.GetOrderDetails;

public record GetOrderDetailsQuery(Guid OrderId) : IRequest<ApiResponse<OrderDetailsDto>>;