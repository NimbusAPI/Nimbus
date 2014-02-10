using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Nimbus.UnitTests
{
    public abstract class SpecificationForAsync<T> where T : class
    {
        public T Subject;

        public abstract Task<T> Given();
        public abstract Task When();

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