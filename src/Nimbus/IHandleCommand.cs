namespace Nimbus
{
    public interface IHandleCommand<TBusCommand>
    {
        void Handle(TBusCommand busCommand);
    }

    public interface IHandleRequest<TBusRequest, TBusResponse> where TBusRequest: BusRequest<TBusRequest, TBusResponse>
    {
        TBusResponse Handle(TBusRequest request);
    }
}