using Common.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdersService.Domain.Dtos;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Features.Orders.Commands.Create;

[Route("/api/orders")]
[ApiController]
public class CreateOrderController(
    ICartServiceClient cartServiceClient,
    IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateOrderAsync(
        DeliveryLocationCreateDto deliveryLocationCreateDto,
        CancellationToken cancellationToken)
    {
        var userId = User.GetId();
        var cartItems = await cartServiceClient.GetCartItemsAsync(userId, cancellationToken);
        
        var command = new CreateOrderCommand(userId, deliveryLocationCreateDto, cartItems.ToList());
        var response = await mediator.Send(command, cancellationToken);
        
        return this.HandleResponse(response);
    }
}