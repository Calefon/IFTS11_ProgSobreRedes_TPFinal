using System.IO;
using System.IO.Compression;

namespace TPHttpServer
{
    public static class Compressor
    {
        public static byte[] Compress(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream())
            {
                
                using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
                {
                    gzipStream.Write(bytes, 0, bytes.Length);
                }
                
                return memoryStream.ToArray();
            }
        }
    }
}
