using System.IO;
using System.IO.Compression;

namespace Nimbus.Infrastructure.BrokeredMessageServices.Compression
{
    public class GzipCompressor : ICompressor
    {
        public byte[] Compress(byte[] input)
        {
            using (var outputStream = new MemoryStream())
            {
                // We need to close the compression stream before we can get the fully realized output
                using (var inputStream = new MemoryStream(input))
                using (var compressionStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    inputStream.CopyTo(compressionStream);
                }

                return outputStream.ToArray();
            }
        }

        public byte[] Decompress(byte[] input)
        {
            using (var inputStream = new MemoryStream(input))
            using (var decompressionStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                decompressionStream.CopyTo(outputStream);
                return outputStream.ToArray();
            }
        }
    }
}