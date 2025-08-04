using MediatR;

namespace NotificationService.Application.Features.Product.NotifyStockExceeded;

public record NotifyProductStockExceededCommand(Guid UserId, int StockQuantity) : IRequest;