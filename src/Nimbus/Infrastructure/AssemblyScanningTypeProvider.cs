using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Filtering;
using Nimbus.Handlers;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;
using Nimbus.MessageContracts;
using Nimbus.MessageContracts.ControlMessages;

namespace Nimbus.Infrastructure
{
    public class AssemblyScanningTypeProvider : ITypeProvider, IValidatableConfigurationSetting
    {
        protected readonly Assembly[] NimbusAssemblies = {typeof (AuditEvent).Assembly, typeof (Bus).Assembly, typeof (IBus).Assembly};

        private readonly Assembly[] _assemblies;
        private readonly ThreadSafeLazy<Type[]> _allInstantiableTypesInScannedAssemblies;
        private readonly ThreadSafeLazy<Type[]> _commandHandlerTypes;
        private readonly ThreadSafeLazy<Type[]> _commandTypes;
        private readonly ThreadSafeLazy<Type[]> _multicastEventHandlerTypes;
        private readonly ThreadSafeLazy<Type[]> _competingEventHandlerTypes;
        private readonly ThreadSafeLazy<Type[]> _eventTypes;
        private readonly ThreadSafeLazy<Type[]> _requestHandlerTypes;
        private readonly ThreadSafeLazy<Type[]> _requestTypes;
        private readonly ThreadSafeLazy<Type[]> _responseTypes;
        private readonly ThreadSafeLazy<Type[]> _multicastRequestHandlerTypes;
        private readonly ThreadSafeLazy<Type[]> _multicastRequestTypes;
        private readonly ThreadSafeLazy<Type[]> _multicastResponseTypes;
        private readonly ThreadSafeLazy<Type[]> _interceptorTypes;
        private readonly ThreadSafeLazy<Type[]> _filterTypes;

        private IEnumerable<Type> AllInstantiableTypesInScannedAssemblies => _allInstantiableTypesInScannedAssemblies.Value;

        public AssemblyScanningTypeProvider(params Assembly[] assemblies)
        {
            _assemblies = new Assembly[0]
                .Union(assemblies)
                .Union(NimbusAssemblies)
                .ToArray();

            _allInstantiableTypesInScannedAssemblies = new ThreadSafeLazy<Type[]>(ScanForAllInstantiableTypes);
            _commandHandlerTypes = new ThreadSafeLazy<Type[]>(ScanForCommandHandlerTypes);
            _commandTypes = new ThreadSafeLazy<Type[]>(ScanForCommandTypes);
            _multicastEventHandlerTypes = new ThreadSafeLazy<Type[]>(ScanForMulticastEventHandlerTypes);
            _competingEventHandlerTypes = new ThreadSafeLazy<Type[]>(ScanForCompetingEventHandlerTypes);
            _eventTypes = new ThreadSafeLazy<Type[]>(ScanForEventTypes);
            _requestHandlerTypes = new ThreadSafeLazy<Type[]>(ScanForRequestHandlerTypes);
            _requestTypes = new ThreadSafeLazy<Type[]>(ScanForRequestTypes);
            _responseTypes = new ThreadSafeLazy<Type[]>(ScanForResponseTypes);
            _multicastRequestHandlerTypes = new ThreadSafeLazy<Type[]>(ScanForMulticastRequestHandlerTypes);
            _multicastRequestTypes = new ThreadSafeLazy<Type[]>(ScanForMulticastRequestTypes);
            _multicastResponseTypes = new ThreadSafeLazy<Type[]>(ScanForMulticastResponseTypes);
            _filterTypes = new ThreadSafeLazy<Type[]>(ScanForFilterTypes);
            _interceptorTypes = new ThreadSafeLazy<Type[]>(ScanForInterceptorTypes);
        }

        public IEnumerable<Type> CommandHandlerTypes => _commandHandlerTypes.Value;
        public IEnumerable<Type> CommandTypes => _commandTypes.Value;
        public IEnumerable<Type> MulticastEventHandlerTypes => _multicastEventHandlerTypes.Value;
        public IEnumerable<Type> CompetingEventHandlerTypes => _competingEventHandlerTypes.Value;
        public IEnumerable<Type> EventTypes => _eventTypes.Value;
        public IEnumerable<Type> RequestHandlerTypes => _requestHandlerTypes.Value;
        public IEnumerable<Type> RequestTypes => _requestTypes.Value;
        public IEnumerable<Type> ResponseTypes => _responseTypes.Value;
        public IEnumerable<Type> MulticastRequestHandlerTypes => _multicastRequestHandlerTypes.Value;
        public IEnumerable<Type> MulticastRequestTypes => _multicastRequestTypes.Value;
        public IEnumerable<Type> MulticastResponseTypes => _multicastResponseTypes.Value;
        public IEnumerable<Type> FilterTypes => _filterTypes.Value;
        public IEnumerable<Type> InterceptorTypes => _interceptorTypes.Value;

        protected virtual Type[] ScanAssembliesForKnownTypes()
        {
            return _assemblies
                .SelectMany(a => a.GetTypes())
                .ToArray();
        }

        private Type[] ScanForAllInstantiableTypes()
        {
            return ScanAssembliesForKnownTypes()
                .Where(t => t.IsInstantiable())
                .ToArray();
        }

        private Type[] ScanForCommandHandlerTypes()
        {
            var types = AllInstantiableTypesInScannedAssemblies
                .Where(t => t.IsClosedTypeOf(typeof (IHandleCommand<>)))
                .ToArray();

            return types;
        }

        private Type[] ScanForCommandTypes()
        {
            var types = AllInstantiableTypesInScannedAssemblies
                .Where(t => typeof (IBusCommand).IsAssignableFrom(t))
                .ToArray();

            return types;
        }

        private Type[] ScanForMulticastEventHandlerTypes()
        {
            var types = AllInstantiableTypesInScannedAssemblies
                .Where(t => t.IsClosedTypeOf(typeof (IHandleMulticastEvent<>)))
                .ToArray();

            return types;
        }

        private Type[] ScanForCompetingEventHandlerTypes()
        {
            var types = AllInstantiableTypesInScannedAssemblies
                .Where(t => t.IsClosedTypeOf(typeof (IHandleCompetingEvent<>)))
                .ToArray();

            return types;
        }

        private Type[] ScanForEventTypes()
        {
            var types = AllInstantiableTypesInScannedAssemblies
                .Where(t => typeof (IBusEvent).IsAssignableFrom(t))
                .ToArray();

            return types;
        }

        private Type[] ScanForRequestHandlerTypes()
        {
            var types = AllInstantiableTypesInScannedAssemblies
                .Where(t => t.IsClosedTypeOf(typeof (IHandleRequest<,>)))
                .ToArray();

            return types;
        }

        private Type[] ScanForRequestTypes()
        {
            var types = AllInstantiableTypesInScannedAssemblies
                .Where(t => t.IsClosedTypeOf(typeof (IBusRequest<,>)))
                .ToArray();

            return types;
        }

        private Type[] ScanForResponseTypes()
        {
            var types = AllInstantiableTypesInScannedAssemblies
                .Where(t => typeof (IBusResponse).IsAssignableFrom(t))
                .ToArray();

            return types;
        }

        private Type[] ScanForMulticastRequestHandlerTypes()
        {
            var types = AllInstantiableTypesInScannedAssemblies
                .Where(t => t.IsClosedTypeOf(typeof (IHandleMulticastRequest<,>)))
                .ToArray();

            return types;
        }

        private Type[] ScanForMulticastRequestTypes()
        {
            var types = AllInstantiableTypesInScannedAssemblies
                .Where(t => t.IsClosedTypeOf(typeof (IBusMulticastRequest<,>)))
                .ToArray();

            return types;
        }

        private Type[] ScanForMulticastResponseTypes()
        {
            var types = AllInstantiableTypesInScannedAssemblies
                .Where(t => typeof (IBusMulticastResponse).IsAssignableFrom(t))
                .ToArray();

            return types;
        }

        private Type[] ScanForFilterTypes()
        {
            var types = AllInstantiableTypesInScannedAssemblies
                .Where(t => typeof(ISubscriptionFilter).IsAssignableFrom(t))
                .ToArray();

            return types;
        }

        private Type[] ScanForInterceptorTypes()
        {
            var types = AllInstantiableTypesInScannedAssemblies
                .Where(t => typeof (IInboundInterceptor).IsAssignableFrom(t) || typeof (IOutboundInterceptor).IsAssignableFrom(t))
                .ToArray();

            return types;
        }

        public IEnumerable<string> Validate()
        {
            var validationErrors = new string[0]
                .Union(CheckForIndirectlyReferencedAssemblies())
                .Union(CheckForDuplicateQueueNames())
                .Union(CheckForNonSerializableMessageTypes())
                .ToArray();

            return validationErrors;
        }

        private IEnumerable<string> CheckForNonSerializableMessageTypes()
        {
            var validationErrors = this.AllMessageContractTypes()
                                       .Where(mt => !mt.IsSerializable())
                                       .Select(mt => "The message contract type {0} is not serializable.".FormatWith(mt.FullName))
                                       .ToArray();

            return validationErrors;
        }

        private IEnumerable<string> CheckForDuplicateQueueNames()
        {
            var pathFactory = new PathFactory(new GlobalPrefixSetting());
            var duplicateQueues = this.AllMessageContractTypes()
                                      .Select(t => new Tuple<string, Type>(pathFactory.QueuePathFor(t), t))
                                      .GroupBy(tuple => tuple.Item1)
                                      .Where(tuple => tuple.Count() > 1)
                                      .ToArray();

            var validationErrors = duplicateQueues
                .Select(tuple => "Some message types ({0}) would result in a duplicate queue name of {1}".FormatWith(string.Join(", ", tuple), tuple.Key))
                .ToArray();

            return validationErrors;
        }

        private IEnumerable<string> CheckForIndirectlyReferencedAssemblies()
        {
            var genericParameterTypes = this.AllClosedGenericHandlerInterfaces()
                                            .SelectMany(ht => ht.GetGenericArguments())
                                            .ToArray();

            var typesFromMissingAssemblies = genericParameterTypes
                .Where(t => !_assemblies.Contains(t.Assembly))
                .ToArray();

            var validationErrors = typesFromMissingAssemblies
                .Select(t => "The message contract type {0} is referenced by one of your handlers but its assembly ({1}) is not included in the list of assemblies to scan."
                            .FormatWith(t.FullName, t.Assembly.FullName))
                .ToArray();

            return validationErrors;
        }
    }
}