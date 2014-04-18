using System;
using System.Collections.Generic;
using Nimbus.ConcurrentCollections;
using Nimbus.Extensions;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure
{
    internal class KnownMessageTypeVerifier : IKnownMessageTypeVerifier
    {
        private readonly ITypeProvider _typeProvider;
        private readonly ThreadSafeLazy<HashSet<Type>> _validRequestTypes;

        public KnownMessageTypeVerifier(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
            _validRequestTypes = new ThreadSafeLazy<HashSet<Type>>(() => new HashSet<Type>(_typeProvider.AllMessageContractTypes()));
        }

        public void AssertValidMessageType(Type messageType)
        {
            if (!_validRequestTypes.Value.Contains(messageType))
                throw new BusException(
                    "The type {0} is not a recognised message type. Ensure it has been registered with the builder with the WithTypesFrom method."
                        .FormatWith(messageType.FullName));
        }
    }
}