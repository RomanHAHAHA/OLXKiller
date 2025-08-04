using Common.Domain.Models.Results;
using MediatR;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.Products.Queries.GetPagedList;

public class GetProductsQueryHandler(
    IProductsRepository productsRepository) : IRequestHandler<GetProductsQuery, PagedList<ProductPagedDto>>
{
    public async Task<PagedList<ProductPagedDto>> Handle(
        GetProductsQuery request, 
        CancellationToken cancellationToken)
    {
        var pagedProducts = await productsRepository.GetProductsAsync(
            request.ProductFilter,
            request.SortParams,
            request.PageParams,
            cancellationToken);

        var dtos = pagedProducts.Items.Select(p => new ProductPagedDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            IsAvailable = p.StockQuantity > 0,
            Rating = p.AverageRating,
            Categories = p.Categories.Select(c => c.Name).ToList(),
            MainImagePath = p.Images
                .Where(i => i.IsMain)
                .Select(i => i.ImagePath)
                .FirstOrDefault() ?? string.Empty,
        }).ToList();

        return new PagedList<ProductPagedDto>(
            dtos, 
            pagedProducts.Page,
            pagedProducts.PageSize,
            pagedProducts.TotalCount);
    }
}