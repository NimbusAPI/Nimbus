using System;
using Conventional;
using Conventional.Conventions;

namespace Nimbus.Tests.Unit.Conventions
{
    public class MustBeInMatchingNamespaceConventionSpecification : ConventionSpecification
    {
        private readonly Func<string, bool> _predicate;

        public MustBeInMatchingNamespaceConventionSpecification(Func<string, bool> predicate)
        {
            _predicate = predicate;
        }

        public override ConventionResult IsSatisfiedBy(Type type)
        {
            var isSatisfied = _predicate.Invoke(type.Namespace);
            if (isSatisfied) return ConventionResult.Satisfied(type.FullName);

            return ConventionResult.NotSatisfied(type.FullName, FailureMessage);
        }

        protected override string FailureMessage => "Namespace does not match given predicate";
    }
}