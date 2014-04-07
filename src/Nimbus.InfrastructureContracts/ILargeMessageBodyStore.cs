using System;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages
{
    public interface ILargeMessageBodyStore
    {
        Task Store(string id, byte[] bytes, DateTimeOffset expiresAfter);
        Task<byte[]> Retrieve(string id);
        Task Delete(string id);
    }
}