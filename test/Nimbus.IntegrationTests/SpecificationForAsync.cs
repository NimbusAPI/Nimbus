using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Nimbus.Tests.Common.Extensions;
using NUnit.Framework;

namespace Nimbus.IntegrationTests
{
    [TestFixture]
    public abstract class SpecificationForAsync<T> where T : class
    {
        protected const int TimeoutSeconds = 30;
        protected readonly TimeSpan Timeout = TimeSpan.FromSeconds(TimeoutSeconds);

        protected T Subject;

        protected abstract Task<T> Given();
        protected abstract Task When();

        private Stopwatch _sw;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            Task.Run(async () =>
                           {
                               Subject = await Given();

                               _sw = Stopwatch.StartNew();
                               await When();
                               _sw.Stop();

                               Console.WriteLine("Elapsed time: {0} seconds", _sw.Elapsed.TotalSeconds);
                           }).Wait();
        }

        [SetUp]
        public virtual void SetUp()
        {
        }

        [TearDown]
        public virtual void TearDown()
        {
            TestLoggingExtensions.LogTestResult();
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            var disposable = Subject as IDisposable;
            if (disposable != null) disposable.Dispose();
            Subject = null;
        }
    }
}