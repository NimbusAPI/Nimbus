namespace Nimbus.InfrastructureContracts
{
    public interface ICompressor
    {
        byte[] Compress(byte[] input);
        byte[] Decompress(byte[] input);
    }
}