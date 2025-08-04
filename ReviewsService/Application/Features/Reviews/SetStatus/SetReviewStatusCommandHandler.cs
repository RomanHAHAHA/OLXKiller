using Common.Application.Options;
using Common.Domain.Enums;
using Common.Domain.Interfaces;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.Review;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using ReviewsService.Domain.Entities;
using ReviewsService.Domain.Interfaces;

namespace ReviewsService.Application.Features.Reviews.SetStatus;

public class SetReviewStatusCommandHandler(
    IReviewsRepository reviewsRepository,
    IPublishEndpoint publisher,
    IHttpUserContext httpContext,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<SetReviewStatusCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(SetReviewStatusCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewsRepository.GetByIdAsync(
            request.UserId,
            request.ProductId,
            cancellationToken);

        if (review is null)
        {
            return ApiResponse.NotFound(nameof(Review));
        }
        
        review.Status = request.Status;
        await OnReviewStatusSet(request, cancellationToken);
        await reviewsRepository.SaveChangesAsync(cancellationToken);
        
        return ApiResponse.Ok();
    }

    private async Task OnReviewStatusSet(SetReviewStatusCommand request, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();
        var serviceName = serviceOptions.Value.Name;
        var reviewUserId = request.UserId;
        var reviewProductId = request.ProductId;
        
        await publisher.Publish(
            new SystemActionEvent
            {
                CorrelationId = correlationId,
                SenderServiceName = serviceName,
                UserId = httpContext.UserId,
                ActionType = ActionType.Update,
                Message = $"Review of user {reviewUserId} on product {reviewProductId} status set: {request.Status}",
            }, cancellationToken);
        
        await publisher.Publish(
            new ReviewStatusUpdatedEvent
            {
                CorrelationId = correlationId,
                SenderServiceName = serviceName,
                ProductId = reviewProductId
            }, 
            cancellationToken);
    }
}