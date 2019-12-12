using System.Collections.Generic;

namespace Nimbus.InfrastructureContracts.PropertyInjection
{
    public interface IRequireMessageProperties
    {
        IDictionary<string, object> MessageProperties { get; set; }
    }
}