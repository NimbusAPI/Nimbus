using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Handlers;
using Nimbus.MessageContracts.ControlMessages;
using Nimbus.PropertyInjection;

namespace Nimbus.ControlMessageHandlers
{
    /// <summary>
    /// IMPORTANT: This check is, by its very nature, non-deterministic. If there are multiple nodes with the same (erroneous) identification
    /// competing to handle messages then EVEN MULTICAST EVENTS will only go to one of those nodes. If that happens to be the node that
    /// published the HeartbeatEvent in the first place then we'll have no way of knowing that there's anything wrong. Any errors that we
    /// do manage to log may well be sporadic.
    /// </summary>
    public class CheckForDuplicateInstancesHandler : IHandleMulticastEvent<HeartbeatEvent>,
                                                     IRequireLogger,
                                                     IRequireSetting<ApplicationNameSetting>,
                                                     IRequireSetting<InstanceNameSetting>
    {
        public ILogger Logger { get; set; }
        ApplicationNameSetting IRequireSetting<ApplicationNameSetting>.Setting { get; set; }
        InstanceNameSetting IRequireSetting<InstanceNameSetting>.Setting { get; set; }

        public async Task Handle(HeartbeatEvent busEvent)
        {
            var applicationName = ((IRequireSetting<ApplicationNameSetting>) this).Setting;
            var instanceName = ((IRequireSetting<InstanceNameSetting>)this).Setting;
            var process = Process.GetCurrentProcess();

            if (busEvent.ApplicationName != applicationName.Value) return;
            if (busEvent.InstanceName != instanceName.Value) return;
            if (busEvent.MachineName == Environment.MachineName && busEvent.ProcessId == process.Id) return;

            Logger.Error(
                "MISCONFIGURATION: Another process on the bus ({ProcessId} on {MachineName}) appears to have the same identifying details of {ApplicationName}/{InstanceName}",
                process.Id,
                busEvent.MachineName,
                applicationName.Value,
                instanceName.Value);
        }
    }
}