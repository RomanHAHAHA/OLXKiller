using Common.Infrastructure.Messaging.Events.Review;
using MassTransit;
using MediatR;
using ProductsService.Application.Features.Products.Commands.UpdateProductRating;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Infrastructure.Messaging.Consumers;

public class ReviewStatusUpdatedConsumer(
    IMediator mediator,
    IReviewsClient reviewsClient) : IConsumer<ReviewStatusUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ReviewStatusUpdatedEvent> context)
    {
        var productId = context.Message.ProductId;
        var productRating = await reviewsClient.GetProductRatingAsync(productId, context.CancellationToken);
        var command = new UpdateProductRatingCommand(productId, productRating);
        
        await mediator.Send(command, context.CancellationToken);
    }
}