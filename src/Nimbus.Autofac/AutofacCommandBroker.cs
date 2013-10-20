namespace Nimbus.Autofac
{
    public class AutofacCommandBroker : ICommandBroker
    {
        public void Dispatch<TBusCommand>(TBusCommand busEvent)
        {
            throw new System.NotImplementedException();
        }
    }
}