namespace OLXKiller.Domain.Extensions;

public static class StreamExtensions
{
    public static byte[] ConvertToBytes(this Stream stream)
    {
        if (stream is null)
        {
            return [];
        }

        stream.Position = 0;

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);

        return memoryStream.ToArray();
    }
}
