using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Nimbus.IntegrationTests
{
    [TestFixture]
    public abstract class SpecificationFor<T> where T : class
    {
        public T Subject;

        public abstract T Given();
        public abstract void When();

        private Stopwatch _sw;

        [SetUp]
        public void SetUp()
        {
            Subject = Given();

            _sw = Stopwatch.StartNew();
            When();
            _sw.Stop();

            Console.WriteLine("Elapsed time: {0} seconds", _sw.Elapsed.TotalSeconds);
        }

        [TearDown]
        public virtual void TearDown()
        {
            Subject = null;
        }
    }
}