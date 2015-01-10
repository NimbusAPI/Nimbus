using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure
{
    public class TypeProviderValidator : IValidatableConfigurationSetting
    {
        private readonly ITypeProvider _provider;

        public TypeProviderValidator(ITypeProvider provider)
        {
            _provider = provider;
        }

        public IEnumerable<string> Validate()
        {
            var validationErrors = new string[0]
                .Union(_provider.ValidateSelf())
                .Union(CheckForDuplicateQueueNames())
                .Union(CheckForNonSerializableMessageTypes())
                .ToArray();

            return validationErrors;
        }

        private IEnumerable<string> CheckForNonSerializableMessageTypes()
        {
            var validationErrors = _provider.AllMessageContractTypes()
                                       .Where(mt => !mt.IsSerializable())
                                       .Select(mt => "The message contract type {0} is not serializable.".FormatWith(mt.FullName))
                                       .ToArray();

            return validationErrors;
        }

        private IEnumerable<string> CheckForDuplicateQueueNames()
        {
            var duplicateQueues = _provider.AllMessageContractTypes()
                                      .Select(t => new Tuple<string, Type>(PathFactory.QueuePathFor(t), t))
                                      .GroupBy(tuple => tuple.Item1)
                                      .Where(tuple => tuple.Count() > 1)
                                      .ToArray();

            var validationErrors = duplicateQueues
                .Select(tuple => "Some message types ({0}) would result in a duplicate queue name of {1}".FormatWith(string.Join(", ", tuple), tuple.Key))
                .ToArray();

            return validationErrors;
        }
    }
}
