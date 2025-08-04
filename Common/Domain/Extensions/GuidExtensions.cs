using System.Security.Cryptography;

namespace Common.Domain.Extensions;

public static class GuidExtensions
{
    public static Guid CombineGuids(this Guid guid1, Guid guid2)
    {
        var bytes = guid1.ToByteArray().Concat(guid2.ToByteArray()).ToArray();
        var hash = SHA256.HashData(bytes);
        return new Guid(hash.Take(16).ToArray());
    }
}