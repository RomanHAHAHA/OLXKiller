using Common.Domain.Models.Results;
using MediatR;
using ProductsService.Domain.Entities;

namespace ProductsService.Application.Features.Products.Queries.GetProductBase;

public record GetProductBaseQuery(Guid ProductId) : IRequest<ApiResponse<Product>>;