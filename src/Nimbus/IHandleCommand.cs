namespace Nimbus
{
    public interface IHandleCommand<TBusCommand>
    {
        void Handle(TBusCommand busCommand);
    }
}