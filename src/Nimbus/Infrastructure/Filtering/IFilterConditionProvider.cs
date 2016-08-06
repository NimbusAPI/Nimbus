using System;
using Nimbus.Filtering.Conditions;

namespace Nimbus.Infrastructure.Filtering
{
    internal interface IFilterConditionProvider
    {
        IFilterCondition GetFilterConditionFor(Type handlerType);
    }
}