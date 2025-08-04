using MediatR;
using ProductsService.Domain.Dtos;

namespace ProductsService.Application.Features.Categories.Queries.GetViewCategories;

public record GetCategoriesQuery : IRequest<List<ShortCategoryDto>>;