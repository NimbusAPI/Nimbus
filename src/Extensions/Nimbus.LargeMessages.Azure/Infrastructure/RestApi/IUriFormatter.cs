namespace Nimbus.LargeMessages.Azure.Infrastructure.RestApi
{
    internal interface IUriFormatter
    {
        string FormatUri(string storageAccessSignature);
    }
}