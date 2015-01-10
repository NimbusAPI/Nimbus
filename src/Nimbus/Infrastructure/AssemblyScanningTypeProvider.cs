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
using Nimbus.Interceptors.Outbound;
using Nimbus.MessageContracts;
using Nimbus.MessageContracts.ControlMessages;

namespace Nimbus.Infrastructure
{
    public class AssemblyScanningTypeProvider : ITypeProvider
    {
        private readonly Assembly[] _nimbusAssemblies = {typeof (AuditEvent).Assembly, typeof(Bus).Assembly, typeof(IBus).Assembly};

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

        private IEnumerable<Type> AllInstantiableTypesInScannedAssemblies
        {
            get { return _allInstantiableTypesInScannedAssemblies.Value; }
        }

        public AssemblyScanningTypeProvider(params Assembly[] assemblies)
        {
            _assemblies = new Assembly[0]
                .Union(assemblies)
                .Union(_nimbusAssemblies)
                .ToArray();

            _allInstantiableTypesInScannedAssemblies = new ThreadSafeLazy<Type[]>(ScanAssembliesForInterestingTypes);
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

        public IEnumerable<Type> ResponseTypes
        {
            get { return _responseTypes.Value; }
        }

        public IEnumerable<Type> MulticastRequestHandlerTypes
        {
            get { return _multicastRequestHandlerTypes.Value; }
        }

        public IEnumerable<Type> MulticastRequestTypes
        {
            get { return _multicastRequestTypes.Value; }
        }

        public IEnumerable<Type> MulticastResponseTypes
        {
            get { return _multicastResponseTypes.Value; }
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

        private Type[] ScanForInterceptorTypes()
        {
            var types = AllInstantiableTypesInScannedAssemblies
                .Where(t => typeof (IInboundInterceptor).IsAssignableFrom(t) || typeof (IOutboundInterceptor).IsAssignableFrom(t))
                .ToArray();

            return types;
        }

        public IEnumerable<string> ValidateSelf()
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