using Common.Application.Options;
using Common.Domain.Enums;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ReviewsService.Domain.Entities;
using ReviewsService.Infrastructure.Persistence;

namespace ReviewsService.Application.Features.Reviews.Create;

public class CreateReviewCommandHandler(
    ReviewsDbContext dbContext, 
    IPublishEndpoint publisher,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<CreateReviewCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        if (!await dbContext.UserSnapshots
                .AnyAsync(u => u.Id == request.UserId, cancellationToken))
        {
            return ApiResponse.NotFound(nameof(UserSnapshot));
        }
        
        if (!await dbContext.ProductSnapshots
                .AnyAsync(u => u.Id == request.ReviewCreateDto.ProductId, cancellationToken))
        {
            return ApiResponse.NotFound(nameof(ProductSnapshot));
        }
        
        var review = Review.FromCreateDto(request.ReviewCreateDto, request.UserId);
        
        await dbContext.Reviews.AddAsync(review, cancellationToken);
        await OnReviewCreated(request, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return ApiResponse.Ok();
    }

    private async Task OnReviewCreated(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        await publisher.Publish(
            new SystemActionEvent
            {
                CorrelationId = Guid.NewGuid(),
                SenderServiceName = serviceOptions.Value.Name,
                UserId = request.UserId,
                ActionType = ActionType.Create,
                Message = $"User {request.UserId} created review on product {request.ReviewCreateDto.ProductId}"
            }, cancellationToken);
    }
}