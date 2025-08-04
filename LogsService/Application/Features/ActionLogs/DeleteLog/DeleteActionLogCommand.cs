using Common.Domain.Models.Results;
using MediatR;

namespace LogsService.Application.Features.ActionLogs.DeleteLog;

public record DeleteActionLogCommand(Guid ActionLogId) : IRequest<ApiResponse>;