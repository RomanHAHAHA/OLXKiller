namespace NotificationService.Domain.Models;

public record TransactionDetails(Guid CorrelationId, string SenderServiceName);