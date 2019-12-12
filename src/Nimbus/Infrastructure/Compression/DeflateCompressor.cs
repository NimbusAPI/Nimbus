using System.IO;
using System.IO.Compression;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.Compression
{
    public class DeflateCompressor : ICompressor
    {
        public byte[] Compress(byte[] input)
        {
            using (var outputStream = new MemoryStream())
            {
                // We need to close the compression stream before we can get the fully realized output
                using (var inputStream = new MemoryStream(input))
                using (var compressionStream = new DeflateStream(outputStream, CompressionMode.Compress))
                {
                    inputStream.CopyTo(compressionStream);
                }

                return outputStream.ToArray();
            }
        }

        public byte[] Decompress(byte[] input)
        {
            using (var inputStream = new MemoryStream(input))
            using (var decompressionStream = new DeflateStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                decompressionStream.CopyTo(outputStream);
                return outputStream.ToArray();
            }
        }
    }
}