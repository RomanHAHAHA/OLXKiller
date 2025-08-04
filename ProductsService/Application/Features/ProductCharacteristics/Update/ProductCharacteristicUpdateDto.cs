namespace ProductsService.Application.Features.ProductCharacteristics.Update;

public record ProductCharacteristicUpdateDto(
    string OldName,
    string NewName,
    string Value);