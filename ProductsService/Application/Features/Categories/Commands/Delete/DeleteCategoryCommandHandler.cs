using Common.Application.Options;
using Common.Domain.Enums;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.Categories.Commands.Delete;

public class DeleteCategoryCommandHandler(
    ICategoriesRepository categoriesRepository,
    IPublishEndpoint publisher,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<DeleteCategoryCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoriesRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return ApiResponse.NotFound(nameof(Category));
        }

        categoriesRepository.Delete(category);
        await OnCategoryDeleted(request, cancellationToken);
        await categoriesRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse.Ok();
    }
    
    private async Task OnCategoryDeleted(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        await publisher.Publish(
            new SystemActionEvent
            {
                CorrelationId = Guid.NewGuid(),
                SenderServiceName = serviceOptions.Value.Name,
                UserId = request.InitiatorUserId,
                ActionType = ActionType.Delete,
                Message = $"Category {request.CategoryId} has been deleted"
            }, 
            cancellationToken);
    }
}