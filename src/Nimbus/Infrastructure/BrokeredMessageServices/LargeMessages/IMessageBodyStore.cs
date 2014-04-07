using System.Threading.Tasks;

namespace Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages
{
    internal interface IMessageBodyStore
    {
        Task Store(string id, byte[] bytes);
        Task<byte[]> Retrieve(string id);
        Task Delete(string id);
    }
}