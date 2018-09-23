using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Nimbus.Tests.Common.Extensions;
using NUnit.Framework;

namespace Nimbus.StressTests
{
    [TestFixture]
    [Timeout(TimeoutSeconds*1000)]
    public abstract class SpecificationForAsync<T> where T : class
    {
        protected const int TimeoutSeconds = 60;
        protected TimeSpan Timeout = TimeSpan.FromSeconds(TimeoutSeconds);

        protected T Subject;

        protected abstract Task<T> Given();
        protected abstract Task When();

        private Stopwatch _sw;

        [TestFixtureSetUp]
        public virtual void TestFixtureSetUp()
        {
            Task.Run(async () =>
                           {
                               Subject = await Given();

                               _sw = Stopwatch.StartNew();
                               try
                               {
                                   await When();
                               }
                               finally
                               {
                                   _sw.Stop();

                                   Console.WriteLine("Elapsed time: {0} seconds", _sw.Elapsed.TotalSeconds);
                               }
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

        [TestFixtureTearDown]
        public virtual void TestFixtureTearDown()
        {
            var disposable = Subject as IDisposable;
            Subject = null;
            disposable?.Dispose();
        }
    }
}