using Common.Domain.Models.Results;
using MediatR;

namespace ProductsService.Application.Features.Products.Queries.GetQuantity;

public record GetQuantityQuery(Guid ProductId) : IRequest<ApiResponse<int>>;