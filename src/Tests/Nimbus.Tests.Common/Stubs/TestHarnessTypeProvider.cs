using System;
using System.Linq;
using System.Reflection;
using Nimbus.Infrastructure;

namespace Nimbus.Tests.Common.Stubs
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

        protected override Type[] ScanAssembliesForKnownTypes()
        {
            var knownTypes = base.ScanAssembliesForKnownTypes();

            var knownTypesInFilteredNamespace = knownTypes
                .Where(IsInFilteredNamespace)
                .Where(t => !t.Name.EndsWith("ThatIsNotReturedByTheTypeProvider"))
                .ToArray();

            return knownTypesInFilteredNamespace;
        }

        private bool IsInFilteredNamespace(Type type)
        {
            if (NimbusAssemblies.Contains(type.Assembly)) return true;

            return _namespaces.Any(ns => (type.Namespace ?? string.Empty).StartsWith(ns));
        }
    }
}