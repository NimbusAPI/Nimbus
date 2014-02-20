using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface IHandleRequest<TBusRequest, TBusResponse> where TBusRequest : IBusRequest<TBusRequest, TBusResponse> where TBusResponse : IBusResponse
    {
        Task<TBusResponse> Handle(TBusRequest request);
    }
}