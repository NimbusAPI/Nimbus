using System.Threading.Tasks;

namespace Nimbus.LargeMessages.Azure.Infrastructure.RestApi
{
    internal interface IRestApiHelper
    {
        Task Upload(string storageKey, byte[] bytes);
        Task Delete(string storageKey);
        Task<byte[]> Retrieve(string storageKey);
    }
}