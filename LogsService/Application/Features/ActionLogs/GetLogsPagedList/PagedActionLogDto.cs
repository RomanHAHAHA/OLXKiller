namespace LogsService.Application.Features.ActionLogs.GetLogsPagedList;

public class PagedActionLogDto
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string CreatedAt { get; set; } = string.Empty;
}