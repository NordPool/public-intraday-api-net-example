using System.IO.Compression;
using System.Text;

namespace NPS.ID.PublicApi.DotNet.Client.Utils;

public static class GzipCompressor
{
    public static string Compress(string text)
    {
        var compressedBytes = Compress(Encoding.UTF8.GetBytes(text));

        return Encoding.UTF8.GetString(compressedBytes);
    }

    public static byte[] Compress(byte[] data)
    {
        using var compressedStream = new MemoryStream();
        using var zipStream = new GZipStream(compressedStream, CompressionMode.Compress);
        
        zipStream.Write(data, 0, data.Length);
        zipStream.Close();

        var compressedBytes = compressedStream.ToArray();

        return Encoding.UTF8.GetBytes(Convert.ToBase64String(compressedBytes));
    }

    public static string Decompress(string text)
    {
        var decompressedBytes = Decompress(Encoding.UTF8.GetBytes(text));

        return Encoding.UTF8.GetString(decompressedBytes);
    }

    public static byte[] Decompress(byte[] data)
    {
        var decodedArray = Convert.FromBase64String(Encoding.UTF8.GetString(data));

        using var compressedStream = new MemoryStream(decodedArray);
        using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        
        zipStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }
}