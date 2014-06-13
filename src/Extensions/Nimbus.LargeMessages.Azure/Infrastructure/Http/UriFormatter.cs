using System;
using Nimbus.LargeMessages.Azure.Configuration.Settings;
using Nimbus.LargeMessages.Azure.Extensions;

namespace Nimbus.LargeMessages.Azure.Infrastructure.Http
{
    internal class UriFormatter : IUriFormatter
    {
        private readonly AzureBlobStorageContainerSharedAccessSignatureSetting _containerSharedAccessSignatureSetting;
        private readonly AzureBlobStorageContainerUriSetting _containerUriSetting;

        public UriFormatter(AzureBlobStorageContainerUriSetting containerUriSetting, AzureBlobStorageContainerSharedAccessSignatureSetting containerSharedAccessSignatureSetting)
        {
            _containerUriSetting = containerUriSetting;
            _containerSharedAccessSignatureSetting = containerSharedAccessSignatureSetting;
        }

        public Uri FormatUri(string storageKey)
        {
            return new Uri(_containerUriSetting.Value.Append(storageKey) + _containerSharedAccessSignatureSetting, UriKind.Absolute);
        }

    }
}