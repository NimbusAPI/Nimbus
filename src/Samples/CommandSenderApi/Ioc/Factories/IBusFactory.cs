using Nimbus;
using System.Reflection;

namespace CommandSenderApi.Ioc.Factories
{
	public interface IBusFactory
	{
		IBus Create(string connectionString, string name);
	}
}
