using System;

namespace Nimbus.Configuration
{
    [Flags]
    public enum MessagePumpTypes
    {
        Default = All,

        None = 0,
        All = -1,

        Response = 1 << 0,
        Request = 1 << 1,
        Command = 1 << 2,
        MulticastRequest = 1 << 3,
        MulticastEvent = 1 << 4,
        CompetingEvent = 1 << 5,
    }
}