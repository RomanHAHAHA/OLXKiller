using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrdersService.Application.Options;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Application.Services;

public class NovaPoshtaClient(
    HttpClient httpClient,
    IOptions<NovaPoshtaOptions> options) : INovaPoshtaClient
{
    public async Task<List<object>> GetAreasAsync(CancellationToken cancellationToken = default)
    {
        var jObjects = await SendRequestAsync(new
        {
            modelName = "Address",
            calledMethod = "getAreas",
            methodProperties = new { }
        }, cancellationToken);

        return Transform(jObjects);
    }

    public async Task<List<object>> GetCitiesAsync(
        string regionRef, 
        CancellationToken cancellationToken = default)
    {
        var jObjects = await SendRequestAsync(new
        {
            modelName = "Address",
            calledMethod = "getCities",
            methodProperties = new { AreaRef = regionRef }
        }, cancellationToken);

        return Transform(jObjects);
    }

    public async Task<List<object>> GetWarehousesAsync(
        string cityRef,
        CancellationToken cancellationToken = default)
    {
        var jObjects = await SendRequestAsync(new
        {
            modelName = "AddressGeneral",
            calledMethod = "getWarehouses",
            methodProperties = new { CityRef = cityRef }
        }, cancellationToken);

        return Transform(jObjects);
    }

    private async Task<List<JObject>> SendRequestAsync(
        object requestBody,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync(
            options.Value.Url, new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8, "application/json"), 
            cancellationToken);

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JObject.Parse(jsonResponse);

        return result["data"]?.ToObject<List<JObject>>() ?? [];
    }

    private List<object> Transform(List<JObject> collection)
    {
        return collection.Select(warehouse => new
        {
            Ref = warehouse["Ref"]?.ToString(),
            Description = warehouse["Description"]?.ToString()
        }).ToList<object>();
    }
}