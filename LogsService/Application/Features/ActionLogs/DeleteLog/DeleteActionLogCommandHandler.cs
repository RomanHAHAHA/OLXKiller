using Common.Domain.Models.Results;
using LogsService.Domain.Entiites;
using LogsService.Domain.Interfaces;
using MediatR;

namespace LogsService.Application.Features.ActionLogs.DeleteLog;

public class DeleteActionLogCommandHandler(
    ILogsRepository logsRepository) : IRequestHandler<DeleteActionLogCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(DeleteActionLogCommand request, CancellationToken cancellationToken)
    {
        var actionLog = await logsRepository.GetByIdAsync(request.ActionLogId, cancellationToken);

        if (actionLog is null)
        {
            return ApiResponse.NotFound(nameof(ActionLog));
        }
        
        logsRepository.Delete(actionLog);
        await logsRepository.SaveChangesAsync(cancellationToken);
        
        return ApiResponse.Ok();
    }
}