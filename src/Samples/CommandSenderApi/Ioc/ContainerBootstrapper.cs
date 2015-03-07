using CommandSenderApi.Ioc.Factories;
using Microsoft.Practices.Unity;
using Nimbus;
using Nimbus.Infrastructure;
using Nimbus.Logger.NLog;
using NLog;
using INLogger = NLog.Interface.ILogger;
using LoggerAdapter = NLog.Interface.LoggerAdapter;

namespace CommandSenderApi.Ioc
{
	public static class ContainerBootstrapper
	{
		public static void RegisterTypes(IUnityContainer container)
		{
			container.RegisterType<Logger>(new ContainerControlledLifetimeManager(), new InjectionFactory(x => LogManager.GetCurrentClassLogger()));
			container.RegisterType<INLogger, LoggerAdapter>(new ContainerControlledLifetimeManager());
			container.RegisterType<ILogger, NLogLogger>(new ContainerControlledLifetimeManager());
			container.RegisterType<IBusFactory, BusFactory>(new ContainerControlledLifetimeManager());
			container.RegisterType<ITypeProvider, AssemblyScanningTypeProvider>(new ContainerControlledLifetimeManager(), new InjectionFactory(x => new AssemblyScanningTypeProvider(GetEntryAssembly())));
			container.RegisterType<IBus, Bus>(new ContainerControlledLifetimeManager(), new InjectionFactory(x => container.Resolve<IBusFactory>()
																														   .Create(Properties.Settings.Default.ServiceBusConnectionString, "FrontEnd")));
		}

		private static System.Reflection.Assembly GetEntryAssembly()
		{
			var httpContext = System.Web.HttpContext.Current;
			if (httpContext == null || httpContext.ApplicationInstance == null)
			{
				return null;
			}

			var type = httpContext.ApplicationInstance.GetType();
			while (type != null && type.Namespace == "ASP")
			{
				type = type.BaseType;
			}

			return type == null ? null : type.Assembly;
		}

	}
}