using Common.Domain.Models.Results;
using MediatR;

namespace ProductsService.Application.Features.Products.Queries.GetProductFullInfo;

public record GetProductInfoQuery(Guid ProductId, Guid UserId) : IRequest<ApiResponse<ProductInfoDto>>;