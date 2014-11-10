using System.Collections.Generic;

namespace Nimbus.PropertyInjection
{
    public interface IRequireMessageProperties
    {
        IDictionary<string, object> MessageProperties { get; set; }
    }
}