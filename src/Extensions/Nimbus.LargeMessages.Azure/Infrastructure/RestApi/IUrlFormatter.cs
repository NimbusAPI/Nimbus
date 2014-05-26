namespace Nimbus.LargeMessages.Azure.Infrastructure.RestApi
{
    internal interface IUrlFormatter
    {
        string FormatUrl(string storageKey);
    }
}