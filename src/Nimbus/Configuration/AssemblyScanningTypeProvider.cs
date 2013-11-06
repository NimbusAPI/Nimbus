using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Configuration
{
    public class AssemblyScanningTypeProvider : ITypeProvider
    {
        private readonly Assembly[] _assemblies;
        private readonly Lazy<Type[]> _allInstantiableTypesInScannedAssemblies;

        private IEnumerable<Type> AllInstantiableTypesInScannedAssemblies
        {
            get { return _allInstantiableTypesInScannedAssemblies.Value; }
        }

        public AssemblyScanningTypeProvider(params Assembly[] assemblies)
        {
            _assemblies = assemblies;

            _allInstantiableTypesInScannedAssemblies = new Lazy<Type[]>(ScanAssembliesForInterestingTypes);
        }

        private Type[] ScanAssembliesForInterestingTypes()
        {
            return _assemblies
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => !t.IsInterface)
                .Where(t => !t.IsAbstract)
                .ToArray();
        }

        public IEnumerable<Type> CommandHandlerTypes
        {
            get
            {
                var types = AllInstantiableTypesInScannedAssemblies
                    .Where(t => t.IsClosedTypeOf(typeof (IHandleCommand<>)))
                    .ToArray();

                return types;
            }
        }

        public IEnumerable<Type> CommandTypes
        {
            get
            {
                var types = AllInstantiableTypesInScannedAssemblies
                    .Where(t => typeof (IBusCommand).IsAssignableFrom(t))
                    .ToArray();

                return types;
            }
        }

        public IEnumerable<Type> MulticastEventHandlerTypes
        {
            get
            {
                var types = AllInstantiableTypesInScannedAssemblies
                    .Where(t => t.IsClosedTypeOf(typeof (IHandleMulticastEvent<>)))
                    .ToArray();

                return types;
            }
        }

        public IEnumerable<Type> CompetingEventHandlerTypes
        {
            get
            {
                var types = AllInstantiableTypesInScannedAssemblies
                    .Where(t => t.IsClosedTypeOf(typeof (IHandleCompetingEvent<>)))
                    .ToArray();

                return types;
            }
        }

        public IEnumerable<Type> EventTypes
        {
            get
            {
                var types = AllInstantiableTypesInScannedAssemblies
                    .Where(t => typeof (IBusEvent).IsAssignableFrom(t))
                    .ToArray();

                return types;
            }
        }

        public IEnumerable<Type> RequestHandlerTypes
        {
            get
            {
                var types = AllInstantiableTypesInScannedAssemblies
                    .Where(t => t.IsClosedTypeOf(typeof (IHandleRequest<,>)))
                    .ToArray();

                return types;
            }
        }

        public IEnumerable<Type> RequestTypes
        {
            get
            {
                var types = AllInstantiableTypesInScannedAssemblies
                    .Where(t => typeof (IBusRequest).IsAssignableFrom(t))
                    .ToArray();

                return types;
            }
        }
    }
}