using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Interceptors;
using Nimbus.Interceptors.Inbound;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure
{
    public class AssemblyScanningTypeProvider : ITypeProvider, IValidatableConfigurationSetting
    {
        private readonly Assembly[] _assemblies;
        private readonly ThreadSafeLazy<Type[]> _allInstantiableTypesInScannedAssemblies;
        private readonly ThreadSafeLazy<Type[]> _commandHandlerTypes;
        private readonly ThreadSafeLazy<Type[]> _commandTypes;
        private readonly ThreadSafeLazy<Type[]> _multicastEventHandlerTypes;
        private readonly ThreadSafeLazy<Type[]> _competingEventHandlerTypes;
        private readonly ThreadSafeLazy<Type[]> _eventTypes;
        private readonly ThreadSafeLazy<Type[]> _requestHandlerTypes;
        private readonly ThreadSafeLazy<Type[]> _requestTypes;
        private readonly ThreadSafeLazy<Type[]> _interceptorTypes;

        private IEnumerable<Type> AllInstantiableTypesInScannedAssemblies
        {
            get { return _allInstantiableTypesInScannedAssemblies.Value; }
        }

        public AssemblyScanningTypeProvider(params Assembly[] assemblies)
        {
            _assemblies = assemblies;

            _allInstantiableTypesInScannedAssemblies = new ThreadSafeLazy<Type[]>(ScanAssembliesForInterestingTypes);
            _commandHandlerTypes = new ThreadSafeLazy<Type[]>(ScanForCommandHandlerTypes);
            _commandTypes = new ThreadSafeLazy<Type[]>(ScanForCommandTypes);
            _multicastEventHandlerTypes = new ThreadSafeLazy<Type[]>(ScanForMulticastEventHandlerTypes);
            _competingEventHandlerTypes = new ThreadSafeLazy<Type[]>(ScanForCompetingEventHandlerTypes);
            _eventTypes = new ThreadSafeLazy<Type[]>(ScanForEventTypes);
            _requestHandlerTypes = new ThreadSafeLazy<Type[]>(ScanForRequestHandlerTypes);
            _requestTypes = new ThreadSafeLazy<Type[]>(ScanForRequestTypes);
            _interceptorTypes = new ThreadSafeLazy<Type[]>(ScanForInterceptorTypes);
        }

        public IEnumerable<Type> CommandHandlerTypes
        {
            get { return _commandHandlerTypes.Value; }
        }

        public IEnumerable<Type> CommandTypes
        {
            get { return _commandTypes.Value; }
        }

        public IEnumerable<Type> MulticastEventHandlerTypes
        {
            get { return _multicastEventHandlerTypes.Value; }
        }

        public IEnumerable<Type> CompetingEventHandlerTypes
        {
            get { return _competingEventHandlerTypes.Value; }
        }

        public IEnumerable<Type> EventTypes
        {
            get { return _eventTypes.Value; }
        }

        public IEnumerable<Type> RequestHandlerTypes
        {
            get { return _requestHandlerTypes.Value; }
        }

        public IEnumerable<Type> RequestTypes
        {
            get { return _requestTypes.Value; }
        }

        public IEnumerable<Type> InterceptorTypes
        {
            get { return _interceptorTypes.Value; }
        }

        protected virtual Type[] ScanAssembliesForInterestingTypes()
        {
            return _assemblies
                .SelectMany(a => a.GetTypes())
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

        private Type[] ScanForInterceptorTypes()
        {
            var types = AllInstantiableTypesInScannedAssemblies
                .Where(t => typeof (IInboundInterceptor).IsAssignableFrom(t))
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
            var duplicateQueues = this.AllMessageContractTypes()
                                      .Select(t => new Tuple<string, Type>(PathFactory.QueuePathFor(t), t))
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