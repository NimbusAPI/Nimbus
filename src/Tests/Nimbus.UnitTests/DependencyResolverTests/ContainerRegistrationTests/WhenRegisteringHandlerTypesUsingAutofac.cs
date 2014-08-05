using System.Threading.Tasks;
using Autofac;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using NUnit.Framework;

namespace Nimbus.UnitTests.DependencyResolverTests.ContainerRegistrationTests
{
    [TestFixture]
    public class WhenRegisteringHandlerTypesUsingAutofac
    {
        [Test]
        public async Task NothingShouldGoBang()
        {
            var typeProvider = new AssemblyScanningTypeProvider(GetType().Assembly);

            var builder = new ContainerBuilder();
            builder.RegisterNimbus(typeProvider);

            using (builder.Build()) { }
        }
    }
}