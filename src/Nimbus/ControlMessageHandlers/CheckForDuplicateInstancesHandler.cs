using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Handlers;
using Nimbus.MessageContracts.ControlMessages;
using Nimbus.PropertyInjection;

namespace Nimbus.ControlMessageHandlers
{
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