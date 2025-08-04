using MediatR;
using ProductsService.Domain.Entities;

namespace ProductsService.Application.Features.Categories.Queries.GetDbCategories;

public record GetDbCategoriesQuery : IRequest<List<Category>>;