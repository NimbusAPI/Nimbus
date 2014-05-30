using Nimbus.Extensions;
using Nimbus.LargeMessages.Azure.Configuration.Settings;

namespace Nimbus.LargeMessages.Azure.Infrastructure.RestApi
{
    internal class UriFormatter : IUriFormatter
    {
        private readonly RestStorageSharedAccessKeySetting _sharedAccessKeySetting;
        private readonly RestStorageUriSetting _uriSetting;

        public UriFormatter(RestStorageUriSetting uriSetting, RestStorageSharedAccessKeySetting sharedAccessKeySetting)
        {
            _uriSetting = uriSetting;
            _sharedAccessKeySetting = sharedAccessKeySetting;
        }

        public string FormatUri(string storageAccessSignature)
        {
            return "{0}/{1}{2}".FormatWith(_uriSetting, storageAccessSignature, _sharedAccessKeySetting);
        }
    }
}