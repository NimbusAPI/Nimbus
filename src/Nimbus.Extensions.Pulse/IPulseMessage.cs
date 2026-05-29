using System;

namespace Nimbus.Extensions.Pulse
{
    public interface IPulseMessage
    {
        DateTimeOffset PulseTime { get; set; }
    }
}
