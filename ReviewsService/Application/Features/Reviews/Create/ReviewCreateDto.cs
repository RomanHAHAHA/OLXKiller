namespace ReviewsService.Application.Features.Reviews.Create;

public record ReviewCreateDto(
    Guid ProductId,
    string Text,
    int Rate);