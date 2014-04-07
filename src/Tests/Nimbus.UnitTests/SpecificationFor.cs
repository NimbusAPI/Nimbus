using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Nimbus.UnitTests
{
    [TestFixture]
    public abstract class SpecificationFor<T> where T : class
    {
        protected T Subject;

        protected abstract T Given();
        protected abstract void When();

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

        protected TimeSpan ElapsedTime
        {
            get { return _sw.Elapsed; }
        }

        [TearDown]
        public virtual void TearDown()
        {
            Subject = null;
        }
    }
}