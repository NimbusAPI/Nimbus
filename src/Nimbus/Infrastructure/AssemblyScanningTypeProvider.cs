using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure
{
    public class AssemblyScanningTypeProvider : ITypeProvider
    {
        private readonly Assembly[] _assemblies;
        private readonly Lazy<Type[]> _allInstantiableTypesInScannedAssemblies;
        private readonly Lazy<Type[]> _commandHandlerTypes;
        private readonly Lazy<Type[]> _commandTypes;
        private readonly Lazy<Type[]> _multicastEventHandlerTypes;
        private readonly Lazy<Type[]> _competingEventHandlerTypes;
        private readonly Lazy<Type[]> _eventTypes;
        private readonly Lazy<Type[]> _requestHandlerTypes;
        private readonly Lazy<Type[]> _requestTypes;

        private IEnumerable<Type> AllInstantiableTypesInScannedAssemblies
        {
            get { return _allInstantiableTypesInScannedAssemblies.Value; }
        }

        public AssemblyScanningTypeProvider(params Assembly[] assemblies)
        {
            _assemblies = assemblies;

            _allInstantiableTypesInScannedAssemblies = new Lazy<Type[]>(ScanAssembliesForInterestingTypes);
            _commandHandlerTypes = new Lazy<Type[]>(ScanForCommandHandlerTypes);
            _commandTypes = new Lazy<Type[]>(ScanForCommandTypes);
            _multicastEventHandlerTypes = new Lazy<Type[]>(ScanForMulticastEventHandlerTypes);
            _competingEventHandlerTypes = new Lazy<Type[]>(ScanForCompetingEventHandlerTypes);
            _eventTypes = new Lazy<Type[]>(ScanForEventTypes);
            _requestHandlerTypes = new Lazy<Type[]>(ScanForRequestHandlerTypes);
            _requestTypes = new Lazy<Type[]>(ScanForRequestTypes);
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

        protected virtual Type[] ScanAssembliesForInterestingTypes()
        {
            return _assemblies
                .SelectMany(a => a.GetExportedTypes())
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

        public void Verify()
        {
            AssertAllHandledMessageTypesAreIncludedDirectly();
            AssertThatWeWontDuplicateQueueNames();
        }

        private void AssertThatWeWontDuplicateQueueNames()
        {
            var queueCounts = new Tuple<string, Type>[0]
                .Union(CommandTypes.Select(t => new Tuple<string, Type>(PathFactory.QueuePathFor(t), t)))
                .Union(RequestTypes.Select(t => new Tuple<string, Type>(PathFactory.QueuePathFor(t), t)))
                .Union(EventTypes.Select(t => new Tuple<string, Type>(PathFactory.TopicPathFor(t), t)))
                .GroupBy(queue => queue.Item1)
                .Where(dupe => dupe.Count() > 1).ToArray();


            if (queueCounts.None())
                return;

            var badTypes = queueCounts.SelectMany(dupe => dupe.Select( d => d.Item2.Name));
            var message = "Your message types {0} will result in a duplicate queue name.".FormatWith(string.Join(", ", badTypes));

            throw new BusException(message);

        }
        

        private void AssertAllHandledMessageTypesAreIncludedDirectly()
        {
            var handlerTypes = this.AllHandlerTypes()
                                   .SelectMany(ht => ht.GetInterfaces())
                                   .Where(t => t.IsClosedTypeOf(typeof (IHandleCommand<>),
                                                                typeof (IHandleCompetingEvent<>),
                                                                typeof (IHandleMulticastEvent<>),
                                                                typeof (IHandleRequest<,>)))
                                   .ToArray();
            var genericParameterTypes = handlerTypes
                .SelectMany(ht => ht.GetGenericArguments())
                .ToArray();

            foreach (var parameterType in genericParameterTypes)
            {
                var assemblyIsInProvidedList = _assemblies.Contains(parameterType.Assembly);

                if (!assemblyIsInProvidedList)
                {
                    var message = "The message contract type {0} is referenced by one of your handlers but its assembly is not included in the list of assemblies to scan."
                        .FormatWith(
                            parameterType.FullName);

                    throw new BusException(message);
                }
            }
        }
    }
}