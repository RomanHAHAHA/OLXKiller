namespace ProductsService.Application.Common.Dtos;

public record ProductCreateDto(
    string Name,
    string Description,
    decimal? Price,
    int? StockQuantity);