using System.Text.Json;
using Common.Domain.Dtos;
using ProductsService.Domain.Interfaces;

namespace ProductsService.Infrastructure.Services;

public class ReviewsClient(HttpClient httpClient) : IReviewsClient
{
    public async Task<double> GetProductRatingAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync(
            $"https://localhost:7247/api/reviews/product/{productId}/rating",
            cancellationToken);
        
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        var wrapper = JsonSerializer.Deserialize<DataWrapper<double>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return wrapper?.Data ?? 0;
    }
}