using Common.Application.Options;
using Common.Domain.Enums;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Extensions;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.Categories.Commands.Update;

public class UpdateCategoryCommandHandler(
    ICategoriesRepository categoriesRepository,
    IPublishEndpoint publishEndpoint,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<UpdateCategoryCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoriesRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return ApiResponse.NotFound(nameof(Category));
        }
        
        category.UpdateFromCreateDto(request.CategoryCreateDto);
        
        try
        {
            await OnCategoryUpdated(request, cancellationToken);
            await categoriesRepository.SaveChangesAsync(cancellationToken);

            return ApiResponse.Ok();
        }
        catch (DbUpdateException exception) when 
            (exception.InnerException is SqlException { Number: 2601 })
        {
            return ApiResponse.Conflict("Category with the same name already exists.");
        }
        catch (Exception e)
        {
            return ApiResponse.InternalServerError(e.Message);
        }
    }

    private async Task OnCategoryUpdated(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            new SystemActionEvent
            {
                CorrelationId = Guid.NewGuid(),
                SenderServiceName = serviceOptions.Value.Name,
                UserId = request.InitiatorUserId,
                ActionType = ActionType.Update,
                Message = $"Category {request.CategoryId} updated"
            }, cancellationToken);
    }
}