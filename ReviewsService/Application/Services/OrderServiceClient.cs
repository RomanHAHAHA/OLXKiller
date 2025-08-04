using System.Net.Http.Headers;
using System.Text.Json;
using Common.Domain.Interfaces;
using Common.Domain.Models.Results;
using ReviewsService.Domain.Interfaces;

namespace ReviewsService.Application.Services;

public class OrderServiceClient(
    HttpClient httpClient,
    IHttpUserContext httpContext) : IOrderServiceClient
{
    public async Task<ApiResponse<bool>> HasUserOrderedProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var token = httpContext.AccessToken; 

        if (string.IsNullOrEmpty(token))
        {
            return ApiResponse<bool>.UnAuthorized();
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = await httpClient.GetAsync(
            $"https://localhost:7230/api/orders/{productId}", 
            cancellationToken);
        
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var responseDto = JsonSerializer.Deserialize<ResponseDto<bool>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var result = responseDto?.Data ?? false;

        return ApiResponse<bool>.Ok(result);
    }
}

public class ResponseDto<T>
{
    public T? Data { get; set; }
}