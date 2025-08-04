namespace OrdersService.Domain.Interfaces;

public interface INovaPoshtaClient
{
    Task<List<object>> GetAreasAsync(CancellationToken cancellationToken = default);
    
    Task<List<object>> GetCitiesAsync(
        string areaRef, 
        CancellationToken cancellationToken = default);
    
    Task<List<object>> GetWarehousesAsync(
        string cityRef, 
        CancellationToken cancellationToken = default);
}