using Nimbus.Extensions;
using Nimbus.LargeMessages.Azure.Configuration.Settings;

namespace Nimbus.LargeMessages.Azure.Infrastructure.RestApi
{
    internal class UrlFormatter : IUrlFormatter
    {
        private readonly RestStorageSharedAccessKeySetting _sharedAccessKeySetting;
        private readonly RestStorageUrlSetting _urlSetting;

        public UrlFormatter(RestStorageUrlSetting urlSetting, RestStorageSharedAccessKeySetting sharedAccessKeySetting)
        {
            _urlSetting = urlSetting;
            _sharedAccessKeySetting = sharedAccessKeySetting;
        }

        public string FormatUrl(string storageKey)
        {
            return "{0}/{1}{2}".FormatWith(_urlSetting, storageKey, _sharedAccessKeySetting);
        }
    }
}