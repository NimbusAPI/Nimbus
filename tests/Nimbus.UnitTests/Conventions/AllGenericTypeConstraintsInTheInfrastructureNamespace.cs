using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Handlers;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class AllGenericTypeConstraintsInTheInfrastructureNamespace
    {
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void MustBeInterfaces(GenericTypeConstraintTestCase testCase)
        {
            testCase.Constraint.IsInterface.ShouldBe(true);
        }

        private class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var referenceType = typeof(IHandleCommand<>);

                var interfaceTypes = referenceType.Assembly.GetTypes()
                                                  .Where(t => t.Namespace.StartsWith(referenceType.Namespace))
                                                  .Where(t => t.IsInterface);

                var testCases = from infrastructureType in interfaceTypes
                                from genericTypeArgument in infrastructureType.GetGenericArguments()
                                from constraint in genericTypeArgument.GetGenericParameterConstraints()
                                select new GenericTypeConstraintTestCase(infrastructureType, genericTypeArgument, constraint);

                return testCases.Select(tc => new TestCaseData(tc)
                                            .SetName(tc.ToString())
                    ).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class GenericTypeConstraintTestCase
        {
            private readonly Type _infrastructureType;
            private readonly Type _genericTypeArgument;
            private readonly Type _constraint;

            public GenericTypeConstraintTestCase(Type infrastructureType, Type genericTypeArgument, Type constraint)
            {
                _infrastructureType = infrastructureType;
                _genericTypeArgument = genericTypeArgument;
                _constraint = constraint;
            }

            public Type InfrastructureType
            {
                get { return _infrastructureType; }
            }

            public Type GenericTypeArgument
            {
                get { return _genericTypeArgument; }
            }

            public Type Constraint
            {
                get { return _constraint; }
            }

            public override string ToString()
            {
                return "{0}<{1}> where {1}: {2}".FormatWith(_infrastructureType.Name, _genericTypeArgument.Name, _constraint.Name);
            }
        }
    }
}