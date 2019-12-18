using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Conventional;
using Conventional.Conventions;

namespace Nimbus.Tests.Unit.Conventions
{
    public class ParameterOrderConventionSpecification : ConventionSpecification
    {
        private readonly Func<IEnumerable<ParameterInfo>, IEnumerable<ParameterInfo>> _sortParameters;

        public ParameterOrderConventionSpecification(Func<IEnumerable<ParameterInfo>, IEnumerable<ParameterInfo>> sortParameters)
        {
            _sortParameters = sortParameters;
        }

        public override ConventionResult IsSatisfiedBy(Type type)
        {
            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var constructor in constructors)
            {
                var actualParameterOrder = constructor.GetParameters();
                var expectedParameterOrder = _sortParameters(actualParameterOrder).ToArray();

                if (actualParameterOrder.SequenceEqual(expectedParameterOrder)) continue;

                var message = $"Expected order of constructor parameters is {expectedParameterOrder.Select(p => p.ParameterType.Name).ToArray().Join(", ")}.";
                return ConventionResult.NotSatisfied(type.FullName, message);
            }

            return ConventionResult.Satisfied(type.FullName);
        }

        protected override string FailureMessage => "Expected order of parameters not satisfied.";
    }
}