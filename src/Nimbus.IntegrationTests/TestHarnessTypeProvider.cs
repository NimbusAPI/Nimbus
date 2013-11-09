using System;
using System.Linq;
using System.Reflection;
using Nimbus.Infrastructure;

namespace Nimbus.IntegrationTests
{
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
                .ToArray();
            return interestingTypesInFilteredNamespaces;
        }
    }
}