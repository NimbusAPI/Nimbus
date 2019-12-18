using Nimbus.InfrastructureContracts.Handlers;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class CoreHandlerInterfaces
    {
        [Test]
        public void MustAdhereToConventions()
        {
            var coreInfrastructureInterfaces = new[]
                                               {
                                                   typeof(IHandleCommand<>),
                                                   typeof(IHandleRequest<,>),
                                                   typeof(IHandleMulticastEvent<>),
                                                   typeof(IHandleCompetingEvent<>)
                                               };

            foreach (var type in coreInfrastructureInterfaces)
            {
                type.Namespace.ShouldBe("Nimbus.InfrastructureContracts.Handlers");
            }
        }
    }
}