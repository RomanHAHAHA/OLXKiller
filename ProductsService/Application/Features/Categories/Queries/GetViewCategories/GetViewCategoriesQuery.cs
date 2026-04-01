using MediatR;

namespace ProductsService.Application.Features.Categories.Queries.GetViewCategories;

public record GetViewCategoriesQuery : IRequest<List<CategoryNodeDto>>;