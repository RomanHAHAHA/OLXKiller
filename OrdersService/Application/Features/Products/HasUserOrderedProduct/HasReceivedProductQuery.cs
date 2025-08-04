using Common.Domain.Models.Results;
using MediatR;

namespace OrdersService.Application.Features.Products.HasUserOrderedProduct;

public record HasReceivedProductQuery(
    Guid UserId,
    Guid ProductId) : IRequest<ApiResponse<bool>>;