using System.Threading.Tasks;

namespace Nimbus.LargeMessages.Azure.Http
{
    internal interface IAzureBlobStorageHttpClient
    {
        Task Upload(string storageKey, byte[] bytes);
        Task Delete(string storageKey);
        Task<byte[]> Retrieve(string storageKey);
    }
}