using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure.DependencyResolverFactories;
using NUnit.Framework;

namespace Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure
{
    public abstract class TestForAllDependencyResolvers
    {
        protected IDependencyResolver Subject { get; private set; }
        private AllDependencyResolversTestContext _context;

        protected virtual async Task Given(AllDependencyResolversTestContext context)
        {
            _context = context;
            Subject = await context.Create();
        }

        protected abstract Task When();

        [TearDown]
        public virtual void TearDown()
        {
            MethodCallCounter.Clear();

            var context = _context;
            if (context == null) return;
            context.TearDown();
        }

        protected IEnumerable<Assembly> AssembliesToScan
        {
            get { yield return typeof (TestForAllDependencyResolvers).Assembly; }
        }

        public IEnumerable<TestCaseData> TestCases()
        {
            var typeProvider = new TestHarnessTypeProvider(new[] {Assembly.GetExecutingAssembly()}, new[] {GetType().Namespace});

            return AssembliesToScan
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => typeof (IDependencyResolverFactory).IsAssignableFrom(t))
                .Where(t => t.IsInstantiable())
                .Select(Activator.CreateInstance)
                .Cast<IDependencyResolverFactory>()
                .Select(factory => new AllDependencyResolversTestContext(factory, typeProvider))
                .Select(context => new TestCaseData(context).SetName(context.TestName))
                ;
        }

        public class AllDependencyResolversTestContext
        {
            private readonly IDependencyResolverFactory _factory;
            private readonly ITypeProvider _typeProvider;

            public AllDependencyResolversTestContext(IDependencyResolverFactory factory, ITypeProvider typeProvider)
            {
                _factory = factory;
                _typeProvider = typeProvider;
            }

            public Task<IDependencyResolver> Create()
            {
                return _factory.Create(_typeProvider);
            }

            public void TearDown()
            {
                _factory.Dispose();
            }

            public string TestName
            {
                get { return _factory.GetType().Name; }
            }
        }
    }
}