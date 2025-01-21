namespace OLXKiller.Domain.Extensions;

public static class StreamExtensions
{
    public async static Task<byte[]> ConvertToBytesAsync(this Stream stream)
    {
        if (stream is null)
        {
            return [];
        }

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        return memoryStream.ToArray();
    }
}
