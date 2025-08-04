using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProductsService.Application.Features.Categories.Queries.GetViewCategories;

[ApiController]
[Route("api/categories")]
public class GetCategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        var query = new GetCategoriesQuery();
        var categories = await mediator.Send(query, cancellationToken);
        return Ok(new { data = categories });
    }
}