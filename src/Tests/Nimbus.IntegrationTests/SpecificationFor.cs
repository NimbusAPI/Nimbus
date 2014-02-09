using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Nimbus.IntegrationTests
{
    [TestFixture]
    public abstract class SpecificationFor<T> where T : class
    {
        public T Subject;

        public abstract Task<T> Given();
        public abstract Task When();

        private Stopwatch _sw;

        [SetUp]
        public void SetUp()
        {
            _sw = Stopwatch.StartNew();
        }

        [TearDown]
        public virtual void TearDown()
        {
            _sw.Stop();

            Console.WriteLine("Elapsed time: {0} seconds", _sw.Elapsed.TotalSeconds);
            Subject = null;
        }
    }
}