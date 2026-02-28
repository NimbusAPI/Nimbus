// using System.Threading.Tasks;
// using Castle.Windsor;
// using Nimbus.Infrastructure;
// using Nimbus.Windsor.Configuration;
// using NUnit.Framework;

// namespace Nimbus.UnitTests.ContainerRegistrationTests
// {
//     [TestFixture]
//     public class WhenRegisteringHandlerTypesUsingWindsor
//     {
//         [Test]
//         public async Task NothingShouldGoBang()
//         {
//             var typeProvider = new AssemblyScanningTypeProvider(GetType().Assembly);

//             using (var container = new WindsorContainer())
//             {
//                 container.RegisterNimbus(typeProvider);
//             }
//         }
//     }
// }