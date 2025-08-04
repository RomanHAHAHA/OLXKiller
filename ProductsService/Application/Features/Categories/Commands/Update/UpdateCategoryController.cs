using Common.API.Authentication;
using Common.API.Extensions;
using Common.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ProductsService.Application.Common.Dtos;

namespace ProductsService.Application.Features.Categories.Commands.Update;

[ApiController]
[Route("api/categories")]
public class UpdateCategoryController(IMediator mediator) : ControllerBase
{
    [HttpPatch("{categoryId:guid}")]
    [HasPermission(PermissionEnum.ManageCategories)]
    public async Task<IActionResult> UpdateCategoryAsync(
        Guid categoryId,
        [FromBody] CategoryCreateDto categoryCreateDto,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(User.GetId(), categoryId, categoryCreateDto);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}