namespace OrdersService.Domain.Dtos;

public record DeliveryLocationCreateDto(
    string Region,
    string City,
    string Warehouse);