namespace Nimbus.Autofac
{
    public class AutofacRequestBroker : IRequestBroker
    {
        public TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request) where TBusRequest : BusRequest<TBusRequest, TBusResponse>
        {
            throw new System.NotImplementedException();
        }
    }
}