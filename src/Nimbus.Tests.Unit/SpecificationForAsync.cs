using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Nimbus.Tests.Unit
{
    public abstract class SpecificationForAsync<T> where T : class
    {
        protected T Subject;

        protected abstract Task<T> Given();
        protected abstract Task When();

        private Stopwatch _sw;

        [SetUp]
        public void SetUp()
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

        [TearDown]
        public virtual void TearDown()
        {
            Subject = null;
        }
    }
}