using System;
using System.Linq;
using System.Reflection;
using Nimbus.InfrastructureContracts.DependencyResolution;
using Nimbus.InfrastructureContracts.Filtering;
using Nimbus.InfrastructureContracts.Filtering.Attributes;
using Nimbus.InfrastructureContracts.Filtering.Conditions;

namespace Nimbus.Infrastructure.Filtering
{
    internal class FilterConditionProvider : IFilterConditionProvider
    {
        private readonly IDependencyResolver _dependencyResolver;

        public FilterConditionProvider(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
        }

        public IFilterCondition GetFilterConditionFor(Type handlerType)
        {
            var filterAttribute = handlerType.GetCustomAttributes<SubscriptionFilterAttribute>().FirstOrDefault();
            if (filterAttribute == null) return new TrueCondition();

            using (var scope = _dependencyResolver.CreateChildScope())
            {
                var filter = (ISubscriptionFilter) scope.Resolve(filterAttribute.FilterType);
                return filter.FilterCondition;
            }
        }
    }
}