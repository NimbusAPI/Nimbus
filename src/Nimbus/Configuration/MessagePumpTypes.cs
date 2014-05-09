using System;

namespace Nimbus.Configuration
{
    [Flags]
    public enum MessagePumpTypes
    {
        Default = Response,
        All = -1,

        None = 0,
        Response = 1,
        Request = 1 << 1,
        Command = 1 << 2,
        MulticastRequest = 1 << 3,
        MulticastEvent = 1 << 4,
        CompetingEvent = 1 << 5,
    }
}