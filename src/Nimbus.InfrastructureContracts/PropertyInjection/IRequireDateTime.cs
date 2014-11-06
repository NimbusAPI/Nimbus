using System;

namespace Nimbus.PropertyInjection
{
    public interface IRequireDateTime
    {
        DateTimeOffset UtcNow { get; set; }
    }
}