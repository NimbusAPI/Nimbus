using System;

namespace Nimbus.LargeMessages.Azure.Http
{
    internal interface IUriFormatter
    {
        Uri FormatUri(string storageKey);
    }
}