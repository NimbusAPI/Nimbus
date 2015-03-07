using Nimbus.MessageContracts;
using System;

namespace CommandSenderApi.Commands
{
	public class GenericBusCommand : IBusCommand
	{
		public Guid Id { get; set; }
		public string Message { get; set; }
	}
}