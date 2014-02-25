using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure.BrokerFactories;
using NUnit.Framework;

namespace Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure
{
    public abstract class TestForAll<TSubject>
    {
        protected TSubject Subject { get; private set; }
        private AllSubjectsTestContext _context;

        protected virtual async Task Given(AllSubjectsTestContext context)
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
            get { yield return typeof (TestForAll<>).Assembly; }
        }

        public IEnumerable<TestCaseData> TestCases()
        {
            var typeProvider = new TestHarnessTypeProvider(new[] {Assembly.GetExecutingAssembly()}, new[] {GetType().Namespace});

            return AssembliesToScan
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => typeof (ICreateMessageHandlerFactory<TSubject>).IsAssignableFrom(t))
                .Select(Activator.CreateInstance)
                .Cast<ICreateMessageHandlerFactory<TSubject>>()
                .Select(factory => new AllSubjectsTestContext(factory, typeProvider))
                .Select(context => new TestCaseData(context).SetName(context.TestName))
                ;
        }

        public class AllSubjectsTestContext
        {
            private readonly ICreateMessageHandlerFactory<TSubject> _factory;
            private readonly ITypeProvider _typeProvider;

            public AllSubjectsTestContext(ICreateMessageHandlerFactory<TSubject> factory, ITypeProvider typeProvider)
            {
                _factory = factory;
                _typeProvider = typeProvider;
            }

            public Task<TSubject> Create()
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