using System.Collections.Generic;

namespace Nimbus.PropertyInjection
{
    public interface IRequireMessageProperties
    {
        Dictionary<string, object> MessageProperties { get; set; }
    }
}