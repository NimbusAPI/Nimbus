using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Configuration
{
    public class AssemblyScanningTypeProvider : ITypeProvider
    {
        private readonly Assembly[] _assemblies;

        public AssemblyScanningTypeProvider(params Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        public IEnumerable<Type> CommandHandlerTypes
        {
            get
            {
                var types = _assemblies
                    .SelectMany(a => a.GetExportedTypes())
                    .Where(t => t.IsClosedTypeOf(typeof (IHandleCommand<>)))
                    .ToArray();

                return types;
            }
        }
        
        public IEnumerable<Type> TimeoutHandlerTypes
        {
            get
            {
                var types = _assemblies
                    .SelectMany(a => a.GetExportedTypes())
                    .Where(t => t.IsClosedTypeOf(typeof (IHandleTimeout<>)))
                    .ToArray();

                return types;
            }
        }

        public IEnumerable<Type> EventHandlerTypes
        {
            get
            {
                var types = _assemblies
                    .SelectMany(a => a.GetExportedTypes())
                    .Where(t => t.IsClosedTypeOf(typeof (IHandleEvent<>)))
                    .ToArray();

                return types;
            }
        }

        public IEnumerable<Type> RequestHandlerTypes
        {
            get
            {
                var types = _assemblies
                    .SelectMany(a => a.GetExportedTypes())
                    .Where(t => t.IsClosedTypeOf(typeof (IHandleRequest<,>)))
                    .ToArray();

                return types;
            }
        }
    }
}