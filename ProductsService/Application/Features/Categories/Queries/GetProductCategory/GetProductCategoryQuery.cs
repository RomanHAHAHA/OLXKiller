using Common.Domain.Models.Results;
using MediatR;
using ProductsService.Domain.Dtos;

namespace ProductsService.Application.Features.Categories.Queries.GetProductCategory;

public record GetProductCategoryQuery(Guid ProductId) : IRequest<ApiResponse<ShortCategoryDto>>;