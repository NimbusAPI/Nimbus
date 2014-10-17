using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Nimbus.Tests.Common;
using NUnit.Framework;

namespace Nimbus.StressTests
{
    public abstract class SpecificationForAsync<T> where T : class
    {
        protected T Subject;

        protected abstract Task<T> Given();
        protected abstract Task When();

        private Stopwatch _sw;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Task.Run(async () =>
                           {
                               MethodCallCounter.Clear();
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
                                   MethodCallCounter.Dump();
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
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            var disposable = Subject as IDisposable;
            if (disposable != null) disposable.Dispose();
            Subject = null;
        }
    }
}