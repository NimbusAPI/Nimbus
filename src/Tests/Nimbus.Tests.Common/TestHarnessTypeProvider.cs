using System;
using System.Linq;
using System.Reflection;
using Nimbus.Infrastructure;

namespace Nimbus.Tests.Common
{
    /// <summary>
    ///     A type provider that filters types we care about to only our own test's namespace. It's a performance optimisation
    ///     because creating and deleting queues and topics is slow on some transports.
    /// </summary>
    public class TestHarnessTypeProvider : AssemblyScanningTypeProvider
    {
        private readonly string[] _namespaces;

        public TestHarnessTypeProvider(Assembly[] assemblies, string[] namespaces) : base(assemblies)
        {
            _namespaces = namespaces;
        }

        protected override Type[] ScanAssembliesForInterestingTypes()
        {
            var interestingTypes = base.ScanAssembliesForInterestingTypes();
            var interestingTypesInFilteredNamespaces = interestingTypes
                .Where(t => _namespaces.Any(ns => (t.Namespace ?? string.Empty).StartsWith(ns)))
                .Where(t => !t.Name.EndsWith("ThatIsNotReturedByTheTypeProvider"))
                .ToArray();
            return interestingTypesInFilteredNamespaces;
        }
    }
}