using Common.Domain.Enums;

namespace Common.Domain.Dtos;

public record SortParams(string? OrderBy, SortDirection? SortDirection);