using System;
using Conventional;
using Conventional.Conventions;

namespace Nimbus.Tests.Unit.Conventions
{
    public class MustBeInternalConventionSpecification : ConventionSpecification
    {
        public override ConventionResult IsSatisfiedBy(Type type)
        {
            if (type.IsPublic) return ConventionResult.NotSatisfied(type.FullName, FailureMessage);

            return ConventionResult.Satisfied(type.FullName);
        }

        protected override string FailureMessage => "Type must be internal";
    }
}