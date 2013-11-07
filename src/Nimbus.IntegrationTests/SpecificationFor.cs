using NUnit.Framework;

namespace Nimbus.IntegrationTests
{
    [TestFixture]
    public abstract class SpecificationFor<T> where T : class
    {
        public T Subject;

        public abstract T Given();
        public abstract void When();

        [SetUp]
        public void SetUp()
        {
            Subject = Given();
            When();
        }

        [TearDown]
        public virtual void TearDown()
        {
            Subject = null;
        }
    }
}