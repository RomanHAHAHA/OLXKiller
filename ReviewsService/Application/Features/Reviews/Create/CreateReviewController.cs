using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewsService.Domain.Interfaces;

namespace ReviewsService.Application.Features.Reviews.Create;

[Route("/api/reviews")]
[ApiController]
public class CreateReviewController(
    IOrderServiceClient orderServiceClient,
    IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReviewAsync(
        ReviewCreateDto reviewCreateDto,
        CancellationToken cancellationToken)
    {
        var orderServiceResponse = await orderServiceClient.HasUserOrderedProductAsync(
            reviewCreateDto.ProductId, 
            cancellationToken);

        if (!orderServiceResponse.Data)
        {
            return Conflict(new { descriprion = "You have not ordered this product" });
        }
            
        var command = new CreateReviewCommand(reviewCreateDto, User.GetId());
        var response = await mediator.Send(command, cancellationToken);
        
        return this.HandleResponse(response);
    }
}