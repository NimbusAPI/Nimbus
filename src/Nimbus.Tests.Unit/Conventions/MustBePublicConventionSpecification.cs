using System;
using Conventional;
using Conventional.Conventions;

namespace Nimbus.Tests.Unit.Conventions
{
    public class MustBePublicConventionSpecification : ConventionSpecification
    {
        public override ConventionResult IsSatisfiedBy(Type type)
        {
            if (type.IsPublic) return ConventionResult.Satisfied(type.FullName);

            return ConventionResult.NotSatisfied(type.FullName, FailureMessage);
        }

        protected override string FailureMessage => "Type must be public";
    }
}