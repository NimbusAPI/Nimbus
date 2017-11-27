using System.Collections.Generic;

namespace Nimbus.Filtering.Conditions
{
    public interface IFilterCondition
    {
        bool IsMatch(IDictionary<string, object> messageProperties);
    }
}