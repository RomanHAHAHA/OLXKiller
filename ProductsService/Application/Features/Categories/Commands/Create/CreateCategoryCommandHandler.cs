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
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.Categories.Commands.Create;

public class CreateCategoryCommandHandler(
    ICategoriesRepository categoriesRepository,
    IPublishEndpoint publishEndpoint,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<CreateCategoryCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = Category.FromCreateDto(request.CategoryCreateDto); 
        await categoriesRepository.CreateAsync(category, cancellationToken);

        try
        {
            await OnCategoryCreated(request, cancellationToken);
            var created = await categoriesRepository.SaveChangesAsync(cancellationToken);

            return created ? ApiResponse.Ok() : ApiResponse.InternalServerError();
        }
        catch (DbUpdateException exception) when 
            (exception.InnerException is SqlException { Number: 2601 })
        {
            return ApiResponse.Conflict("Category with the same name already exists.");
        }
        catch (Exception)
        {
            return ApiResponse.InternalServerError();
        }
    }

    private async Task OnCategoryCreated(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            new SystemActionEvent
            {
                CorrelationId = Guid.NewGuid(),
                SenderServiceName = serviceOptions.Value.Name,
                UserId = request.InitiatorUserId,
                ActionType = ActionType.Create,
                Message = $"Category \"{request.CategoryCreateDto.Name}\" created"
            }, cancellationToken);
    }
}