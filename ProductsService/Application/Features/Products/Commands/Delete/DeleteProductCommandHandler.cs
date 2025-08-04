using Common.Application.Options;
using Common.Domain.Enums;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.Product;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Application.Features.Products.Commands.Delete;

public class DeleteProductCommandHandler(
    IProductsRepository productsRepository,
    IPublishEndpoint publishEndpoint,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<DeleteProductCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productsRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return ApiResponse.NotFound(nameof(Product));
        }
        
        productsRepository.Delete(product);
        await OnProductDeleted(request, cancellationToken);
        
        await productsRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse.Ok();
    }

    private async Task OnProductDeleted(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();
        var serviceName = serviceOptions.Value.Name;
        
        await publishEndpoint.Publish(
            new SystemActionEvent
            {
                CorrelationId = correlationId,
                SenderServiceName = serviceName,
                UserId = request.InitiatorUserId,
                ActionType = ActionType.Delete,
                Message = $"Product {request.ProductId} deleted"
            }, 
            cancellationToken);
        
        await publishEndpoint.Publish(
            new ProductDeletedEvent
            {
                CorrelationId = correlationId,
                SenderServiceName = serviceName,
                ProductId = request.ProductId,
            }, 
            cancellationToken);
    }
}