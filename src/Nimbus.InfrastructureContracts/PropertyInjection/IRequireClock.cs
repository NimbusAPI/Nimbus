namespace Nimbus.PropertyInjection
{
    public interface IRequireClock
    {
        IClock Clock { get; set; }
    }
}