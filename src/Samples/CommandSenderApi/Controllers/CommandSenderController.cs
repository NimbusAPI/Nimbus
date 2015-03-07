using CommandSenderApi.Commands;
using Nimbus;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using INLogger = NLog.Interface.ILogger;

namespace CommandSenderApi.Controllers
{
	[RoutePrefix("api/commandSender")]
	public class CommandSenderController : ApiController
	{
		private readonly IBus _bus;
		private readonly NLog.Interface.ILogger _logger;

		public CommandSenderController(IBus bus, INLogger logger)
		{
			_bus = bus;
			_logger = logger;
		}

		public async Task<IHttpActionResult> Post([FromBody]string message)
		{
			await _bus.Send(new GenericBusCommand { Id = Guid.NewGuid(), Message = message });
			return Ok();
		}
	}
}
