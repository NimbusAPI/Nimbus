using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.BrokeredMessageFactoryTests
{
    [TestFixture]
    internal class WhenCloningABrokeredMessage : GivenABrokeredMessageFactory
    {
        private BrokeredMessage _message;
        private BrokeredMessage _clone;

        protected override async Task When()
        {
            var body = "dummy body";
            _message = await Subject.Create(body);
            _clone = _message.Clone();
        }

        [Test]
        public void NothingShouldGoBang()
        {
        }

        [Test]
        public async Task TheCloneShouldNotBeNull()
        {
            _clone.ShouldNotBe(null);
        }
    }
}