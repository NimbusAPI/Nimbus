using Cronos;

namespace Nimbus.Extensions.Pulse
{
    internal record PulseScheduleEntry(CronExpression Cron, object Message, bool IsCommand);
}
