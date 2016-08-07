using Nimbus.Filtering.Conditions;

namespace Nimbus.Transports.WindowsServiceBus.Filtering
{
    internal interface ISqlFilterExpressionGenerator
    {
        string GenerateFor(IFilterCondition filterCondition);
    }
}