using System;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.IntegrationTests;
using Nimbus.LargeMessages.Azure.Infrastructure;
using Nimbus.Logger;
using Ninject;
using NUnit.Framework;

namespace Nimbus.Extensions.IntegrationTests
{
	[TestFixture]
	public class WhenBuildingABusWithNinjectAndAzureMessageStorage
	{
		[Test]
		public void NothingShouldGoBang()
		{
			using (var kernel = new StandardKernel())
			{
				var typeProvider = new AssemblyScanningTypeProvider();

				kernel.Bind<ILogger>().To<ConsoleLogger>().InSingletonScope();

				kernel.RegisterNimbus(typeProvider);

				kernel.Bind<ILargeMessageBodyStore>().ToMethod<ILargeMessageBodyStore>(
					c => new BlobStorageBuilder()
						.Configure()
						.WithBlobStorageConnectionString(CommonResources.BlobStorageConnectionString)
						.WithLogger(c.Kernel.Get<ILogger>())
						.Build())
				      .InSingletonScope();

				kernel.Bind<IBus>().ToMethod<IBus>(
					c => new BusBuilder().Configure()
					                     .WithNames("IntegrationTestHarness", Environment.MachineName)
					                     .WithConnectionString(
											 @"Endpoint=sb://azaia-test-bus-ns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=hHQFVO3i5+y8tXtrSRbnEb03xcZN0pb4K5G9y3hNDzs=")
					                     .WithTypesFrom(typeProvider)
					                     .WithDefaultTimeout(TimeSpan.FromSeconds(10))
					                     .WithLogger(c.Kernel.Get<ILogger>())
					                     .Build())
				      .InSingletonScope();

				kernel.Get<IBus>();
			}
		}
	}
}