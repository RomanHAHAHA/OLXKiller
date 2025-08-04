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

namespace ProductsService.Application.Features.Products.Commands.Create;

public class CreateProductCommandHandler(
    IProductsRepository productsRepository,
    IPublishEndpoint publishEndpoint,
    IOptions<ServiceOptions> serviceOptions) : IRequestHandler<CreateProductCommand, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.FromCreateDto(request.ProductCreateDto, request.InitiatorUserId);
        
        await productsRepository.CreateAsync(product, cancellationToken);
        await OnProductCreated(product, cancellationToken);
        
        var created = await productsRepository.SaveChangesAsync(cancellationToken);

        return created ? ApiResponse<Guid>.Ok(product.Id) : ApiResponse<Guid>.InternalServerError();
    }

    private async Task OnProductCreated(Product product, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();
        var serviceName = serviceOptions.Value.Name;
        
        await publishEndpoint.Publish(
            new SystemActionEvent
            {
                CorrelationId = correlationId,
                SenderServiceName = serviceName,
                UserId = product.UserId,
                ActionType = ActionType.Create,
                Message = $"Product {product.Id} created"
            },
            cancellationToken);

        await publishEndpoint.Publish(
            new ProductCreatedEvent
            {
                CorrelationId = correlationId,
                SenderServiceName = serviceName,
                ProductId = product.Id,
                SellerId = product.UserId,
                Name = product.Name,
                Price = product.Price,
            }, 
            cancellationToken);
    }
}