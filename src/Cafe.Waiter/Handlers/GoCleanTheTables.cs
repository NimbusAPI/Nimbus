using System.Threading.Tasks;
using Cafe.Messages;
using Nimbus.InfrastructureContracts.Handlers;
using Serilog;

namespace Waiter.Handlers;

public class GoCleanTheTables : IHandleCommand<CleanTheTablesCommand>
{
    public Task Handle(CleanTheTablesCommand busCommand)
    {
        Log.Information("Picking up empty cups and wiping tables at {time}", busCommand.PulseTime);
        return Task.CompletedTask;
    }
}