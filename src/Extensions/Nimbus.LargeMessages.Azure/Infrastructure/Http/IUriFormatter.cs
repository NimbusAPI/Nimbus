using System;

namespace Nimbus.LargeMessages.Azure.Infrastructure.Http
{
    internal interface IUriFormatter
    {
        Uri FormatUri(string storageKey);
    }
}