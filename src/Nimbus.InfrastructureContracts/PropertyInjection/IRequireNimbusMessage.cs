
using Nimbus.Infrastructure;

namespace Nimbus.PropertyInjection
{
    public interface IRequireNimbusMessage
    {
        NimbusMessage NimbusMessage { get; set; }
    }
}