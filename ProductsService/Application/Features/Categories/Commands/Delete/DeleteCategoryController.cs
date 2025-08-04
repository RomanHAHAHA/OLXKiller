using Common.API.Authentication;
using Common.API.Extensions;
using Common.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace ProductsService.Application.Features.Categories.Commands.Delete;

[ApiController]
[Route("api/categories")]
public class DeleteCategoryController(IMediator mediator) : ControllerBase
{
    [HttpDelete("{categoryId:guid}")]
    [HasPermission(PermissionEnum.ManageCategories)]
    public async Task<IActionResult> DeleteCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCategoryCommand(User.GetId(), categoryId);
        var response = await mediator.Send(command, cancellationToken);
        return this.HandleResponse(response);
    }
}