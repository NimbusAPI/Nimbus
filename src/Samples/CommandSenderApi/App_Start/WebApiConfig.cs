using CommandSenderApi.Ioc;
using Microsoft.Practices.Unity;
using System.Web.Http;

namespace CommandSenderApi
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			var container = new UnityContainer();
			ContainerBootstrapper.RegisterTypes(container);
			config.DependencyResolver = new UnityResolver(container);

			// Web API routes
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);
		}
	}
}
