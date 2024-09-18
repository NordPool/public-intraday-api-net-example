using System.IO.Compression;
using System.Text;

namespace NPS.ID.PublicApi.Client.Utils;

public static class GzipCompressor
{
    public static byte[] Compress(byte[] data)
    {
        using var compressedStream = new MemoryStream();
        using var zipStream = new GZipStream(compressedStream, CompressionMode.Compress);
        
        zipStream.Write(data, 0, data.Length);
        zipStream.Close();

        var compressedBytes = compressedStream.ToArray();

        return Encoding.UTF8.GetBytes(Convert.ToBase64String(compressedBytes));
    }
}