using Common.Domain.Abstractions;
using Common.Domain.Enums;

namespace LogsService.Domain.Entiites;

public class ActionLog : Entity<Guid>
{
    public Guid UserId { get; set; }
    
    public ActionType ActionType { get; set; }

    public string Description { get; set; } = string.Empty;
}