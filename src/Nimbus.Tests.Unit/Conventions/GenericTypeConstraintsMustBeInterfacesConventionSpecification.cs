using System;
using System.Linq;
using Conventional;
using Conventional.Conventions;

namespace Nimbus.Tests.Unit.Conventions
{
    public class GenericTypeConstraintsMustBeInterfacesConventionSpecification : ConventionSpecification
    {
        public override ConventionResult IsSatisfiedBy(Type type)
        {
            var tuples = from genericTypeArgument in type.GetGenericArguments()
                         from constraint in genericTypeArgument.GetGenericParameterConstraints()
                         select new {genericTypeArgument, constraint};

            foreach (var tuple in tuples)
            {
                if (tuple.constraint.IsInterface) continue;

                return ConventionResult.NotSatisfied(type.FullName, $"Type constraint {tuple.constraint} is not an interface type");
            }

            return ConventionResult.Satisfied(type.FullName);
        }

        protected override string FailureMessage => "Type constraint is not an interface type";
    }
}