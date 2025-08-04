
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace Common.Domain.Dtos;

public sealed record Cursor(DateTime Date, Guid LastId)
{
    public static string Encode(DateTime date, Guid lastId)
    {
        var cursor =  new Cursor(date, lastId);
        var json = JsonSerializer.Serialize(cursor);
        return Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(json));
    }

    public static Cursor? Decode(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return null;
        }

        try
        {
            var json = Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(cursor));
            return JsonSerializer.Deserialize<Cursor>(json);
        }
        catch
        {
            return null;
        }
    }
}