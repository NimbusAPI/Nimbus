using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus.Handlers
{
    public interface IHandleMulticastRequest<TBusRequest, TBusResponse>
        where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
        where TBusResponse : IBusMulticastResponse
    {
        Task<TBusResponse> Handle(TBusRequest request);
    }
}