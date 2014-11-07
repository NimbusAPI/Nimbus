using System;

namespace Nimbus.PropertyInjection
{
    public interface IRequireDateTime
    {
        Func<DateTimeOffset> UtcNow { get; set; }
    }
}