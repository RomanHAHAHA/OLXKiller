using Common.API.Authentication;
using Common.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ProductsService.Application.Features.Categories.Queries.GetDbCategories;

[ApiController]
[Route("api/categories")]
public class GetAllDbCategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet("db")]
    [HasPermission(PermissionEnum.ManageCategories)]
    public async Task<IActionResult> GetAllDbCategories(CancellationToken cancellationToken)
    {
        var query = new GetDbCategoriesQuery();
        var categories = await mediator.Send(query, cancellationToken);
        return Ok(new { data = categories });
    }
}