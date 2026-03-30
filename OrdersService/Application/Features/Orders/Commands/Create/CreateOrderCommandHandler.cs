using System.Data;
using Common.Application.Options;
using Common.Domain.Dtos;
using Common.Domain.Enums;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.Order;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Features.Orders.Commands.Create;

public class CreateOrderCommandHandler(
    IOrdersRepository ordersRepository,
    IUsersRepository usersRepository,
    IPublishEndpoint publishEndpoint,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<CreateOrderCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.CartItems.Count == 0)
        {
            return ApiResponse.BadRequest("You must provide at least one item.");
        }
        
        var user = await usersRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return ApiResponse.NotFound(nameof(UserSnapshot));
        }

        await using var transaction = await ordersRepository
            .BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            var order = new Order(request.UserId);
        
            await ordersRepository.CreateAsync(order, cancellationToken);

            order.AddDeliveryLocation(request.DeliveryLocationDto);
            order.AddOrderItems(request.CartItems);
        
            await OnOrderCreated(order, user, request.CartItems, cancellationToken);

            await ordersRepository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return ApiResponse.Ok();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            return ApiResponse.InternalServerError();
        }
    }

    private async Task OnOrderCreated(
        Order order,
        UserSnapshot user, 
        List<CartItemDto> cartItems, 
        CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();
        var serviceName = serviceOptions.Value.Name;
        
        await publishEndpoint.Publish(
            new SystemActionEvent
            {
                CorrelationId = correlationId,
                SenderServiceName = serviceName,
                UserId = order.UserId,
                ActionType = ActionType.Create,
                Message = $"Order {order.Id} created"
            }, 
            cancellationToken);
        
        await publishEndpoint.Publish(
            new OrderCreatedEvent
            {
                CorrelationId = correlationId,
                SenderServiceName = serviceName,
                UserId = order.UserId,
                Email = user.Email,
                OrderId = order.Id,
                CartItems = cartItems,
            }, 
            cancellationToken);
    }
}