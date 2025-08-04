using Common.Infrastructure.Messaging.Events.Product;
using MediatR;

namespace NotificationService.Application.Features.Product.NotifyProductsReservationFailed;

public record NotifyProductsReservationFailedCommand(
    Guid UserId,
    List<ProductStockInfo> ProductStockInfos) : IRequest;