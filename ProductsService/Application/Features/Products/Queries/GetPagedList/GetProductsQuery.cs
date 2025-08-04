using Common.Domain.Dtos;
using Common.Domain.Models.Results;
using MediatR;

namespace ProductsService.Application.Features.Products.Queries.GetPagedList;

public record GetProductsQuery(
    ProductFilter ProductFilter,
    SortParams SortParams,
    PageParams PageParams) : IRequest<PagedList<ProductPagedDto>>;
