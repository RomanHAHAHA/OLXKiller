using Common.API.Extensions;
using Common.Domain.Dtos;
using Common.Domain.Models.Results;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace ProductsService.Application.Features.Products.Queries.GetPagedList;

[ApiController]
[Route("api/products")]
public class GetPagedProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<PagedList<ProductPagedDto>> GetPagedProductsAsync(
        [FromQuery] ProductFilter productFilter,
        [FromQuery] SortParams sortParams,
        [FromQuery] PageParams pageParams,
        CancellationToken cancellationToken)
    {
        var userId = User.GetId();
        
        if (userId != Guid.Empty)
        {
            productFilter.UserId = userId;
        }
        
        var command = new GetProductsQuery(productFilter, sortParams, pageParams);
        return await mediator.Send(command, cancellationToken);
    }
}