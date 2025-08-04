using MediatR;
using ProductsService.Domain.Dtos;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.Categories.Queries.GetViewCategories;

public class GetCategoriesQueryHandler(
    ICategoriesRepository categoriesRepository) : IRequestHandler<GetCategoriesQuery, List<ShortCategoryDto>>
{
    public async Task<List<ShortCategoryDto>> Handle(
        GetCategoriesQuery request, 
        CancellationToken cancellationToken)
    {
        var categories = await categoriesRepository.GetAllAsync(cancellationToken);
        
        return categories
            .Select(c => new ShortCategoryDto(c.Id, c.Name))
            .ToList();
    }
}