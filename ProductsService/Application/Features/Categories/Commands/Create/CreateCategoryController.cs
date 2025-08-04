using Common.API.Authentication;
using Common.API.Extensions;
using Common.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductsService.Application.Common.Dtos;

namespace ProductsService.Application.Features.Categories.Commands.Create;

[ApiController]
[Route("api/categories")]
public class CreateCategoryController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [HasPermission(PermissionEnum.ManageCategories)]
    public async Task<IActionResult> CreateCategoryAsync(
        [FromBody] CategoryCreateDto categoryCreateDto,
        CancellationToken cancellationToken)
    {
        var command = new CreateCategoryCommand(User.GetId(), categoryCreateDto);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}