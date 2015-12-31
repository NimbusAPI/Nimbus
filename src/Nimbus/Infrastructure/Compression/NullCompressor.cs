namespace Nimbus.Infrastructure.Compression
{
    public class NullCompressor : ICompressor
    {
        public byte[] Compress(byte[] input)
        {
            return input;
        }

        public byte[] Decompress(byte[] input)
        {
            return input;
        }
    }
}