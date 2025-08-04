using System.Text.RegularExpressions;

namespace Common.Domain.Extensions;

public static class StringExtensions
{
    public static string FormatEntityName(this string entityName)
    {
        if (entityName.EndsWith("Entity"))
        {
            entityName = entityName[..^"Entity".Length];
        }

        var words = Regex
            .Matches(entityName, @"[A-Z][a-z]*|[0-9]+")
            .Select(m => m.Value.ToLower());

        return string.Join(" ", words);
    }
}