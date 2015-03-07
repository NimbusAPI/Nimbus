using Microsoft.Practices.Unity;
using Nimbus;
using Nimbus.Configuration;
using Nimbus.Unity.Configuration;
using System;

namespace CommandSenderApi.Ioc.Factories
{
	public class BusFactory : IBusFactory, IDisposable
	{
		private readonly IUnityContainer _container;
		private readonly ITypeProvider _typeProvider;
		private Bus _bus;

		public BusFactory(IUnityContainer container, ITypeProvider typeProvider)
		{
			_container = container;
			_typeProvider = typeProvider;
		}

		public Nimbus.IBus Create(string connectionString, string name)
		{
			_bus = new BusBuilder().Configure()
								   .WithConnectionString(connectionString)
								   .WithNames(name, Environment.MachineName)
								   .WithTypesFrom(_typeProvider)
								   .WithDefaultTimeout(TimeSpan.FromSeconds(30))
								   .WithUnityDependencyResolver(_typeProvider, _container)
								   .Build();
			_bus.Start();

			return _bus;
		}

		public void Dispose()
		{
			if (_bus != null)
			{
				_bus.Dispose();
			}
		}
	}
}