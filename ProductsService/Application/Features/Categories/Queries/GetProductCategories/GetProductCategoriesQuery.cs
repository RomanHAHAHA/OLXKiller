using MediatR;
using ProductsService.Domain.Dtos;

namespace ProductsService.Application.Features.Categories.Queries.GetProductCategories;

public record GetProductCategoriesQuery(Guid ProductId) : IRequest<List<ShortCategoryDto>>;