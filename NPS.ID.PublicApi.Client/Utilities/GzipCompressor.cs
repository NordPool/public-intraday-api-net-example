/*
 *  Copyright 2017 Nord Pool.
 *  This library is intended to aid integration with Nord Pool’s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Client.Utilities
{
    public static class GzipCompressor
    {
        public static string Compress(string text)
        {
            var compressedBytes = Compress(Encoding.UTF8.GetBytes(text));

            return Encoding.UTF8.GetString(compressedBytes);
        }

        public static byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();

                var compressedBytes = compressedStream.ToArray();

                return Encoding.UTF8.GetBytes(Convert.ToBase64String(compressedBytes));
            }
        }

        public static string Decompress(string text)
        {
            var decompressedBytes = Decompress(Encoding.UTF8.GetBytes(text));

            return Encoding.UTF8.GetString(decompressedBytes);
        }

        public static byte[] Decompress(byte[] data)
        {
            var decodedArray = Convert.FromBase64String(Encoding.UTF8.GetString(data));

            using (var compressedStream = new MemoryStream(decodedArray))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }
    }
}
