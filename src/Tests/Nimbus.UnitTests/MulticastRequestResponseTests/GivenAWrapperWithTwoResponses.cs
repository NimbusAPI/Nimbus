using System;
using Nimbus.Infrastructure.RequestResponse;
using NUnit.Framework;

namespace Nimbus.UnitTests.MulticastRequestResponseTests
{
    [TestFixture]
    internal abstract class GivenAWrapperWithTwoResponses : SpecificationFor<MulticastRequestResponseCorrelationWrapper<string>>
    {
        protected override MulticastRequestResponseCorrelationWrapper<string> Given()
        {
            var wrapper = new MulticastRequestResponseCorrelationWrapper<string>(DateTimeOffset.MaxValue);
            wrapper.Reply("Hello!");
            wrapper.Reply("Goodbye!");
            return wrapper;
        }
    }
}