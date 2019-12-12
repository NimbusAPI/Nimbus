using System;
using Nimbus.InfrastructureContracts.Filtering.Conditions;

namespace Nimbus.Infrastructure.Filtering
{
    internal interface IFilterConditionProvider
    {
        IFilterCondition GetFilterConditionFor(Type handlerType);
    }
}