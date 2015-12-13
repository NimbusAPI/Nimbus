using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.PropertyInjection;

namespace Nimbus.Interceptors.Inbound
{
    internal class InboundInterceptorFactory : IInboundInterceptorFactory
    {
        private readonly GlobalInboundInterceptorTypesSetting _globalInboundInterceptorTypes;
        private readonly IPropertyInjector _propertyInjector;

        public InboundInterceptorFactory(GlobalInboundInterceptorTypesSetting globalInboundInterceptorTypes, IPropertyInjector propertyInjector)
        {
            _globalInboundInterceptorTypes = globalInboundInterceptorTypes;
            _propertyInjector = propertyInjector;
        }

        public IInboundInterceptor[] CreateInterceptors(IDependencyResolverScope scope, object handler, object message, NimbusMessage nimbusMessage)
        {
            var globalInterceptors = GetGlobalInterceptorTypes();
            var classLevelInterceptors = GetClassLevelInterceptorTypes(handler);
            var methodLevelInterceptors = GetMethodLevelInterceptorTypes(handler, message);

            var interceptors = new Type[0]
                .Union(globalInterceptors)
                .Union(classLevelInterceptors)
                .Union(methodLevelInterceptors)
                .DistinctBy(t => t.FullName)
                .Select(t => (IInboundInterceptor) scope.Resolve(t))
                .Do(interceptor => _propertyInjector.Inject(interceptor, nimbusMessage))
                .OrderByDescending(i => i.Priority)
                .ThenBy(i => i.GetType().FullName)
                .ToArray();

            return interceptors;
        }

        private IEnumerable<Type> GetGlobalInterceptorTypes()
        {
            var globalInterceptorTypes = _globalInboundInterceptorTypes.Value.ToArray();
            return globalInterceptorTypes;
        }

        private static IEnumerable<Type> GetClassLevelInterceptorTypes(object handler)
        {
            var classHierarchy = new[] {handler.GetType()}.DepthFirst(t => new[] {t.BaseType}.NotNull()).ToArray();

            var classLevelInterceptorTypes = classHierarchy
                .SelectMany(t => t.GetCustomAttributes<InterceptorAttribute>())
                .Select(attr => attr.InterceptorType)
                .ToArray();

            return classLevelInterceptorTypes;
        }

        private static IEnumerable<Type> GetMethodLevelInterceptorTypes(object handler, object message)
        {
            var messageType = message.GetType();

            var implicitImplementations = handler.GetType()
                                                 .GetMethods()
                                                 .Where(m => string.Compare(m.Name, "Handle", StringComparison.OrdinalIgnoreCase) == 0)
                                                 .Where(m => m.GetParameters().Count() == 1)
                                                 .Where(m => m.GetParameters().First().ParameterType == messageType)
                                                 .ToArray();

            var explicitImplementations = handler.GetType()
                                                 .GetInterfaces()
                                                 .SelectMany(@interface => @interface.GetMethods())
                                                 .Where(m => string.Compare(m.Name, "Handle", StringComparison.OrdinalIgnoreCase) == 0)
                                                 .Where(m => m.GetParameters().Count() == 1)
                                                 .Where(m => m.GetParameters().First().ParameterType == messageType)
                                                 .ToArray();

            var methods = implicitImplementations.Concat(explicitImplementations).ToArray();

            // This will take the method on the concrete type if there is one (so that we can traverse the inheritance
            // graph), otherwise it will take the explicit interface method.
            var method = methods.First();

            var methodHierarchy = new[] {method}.DepthFirst(m => new[] {m.GetBaseDefinition()}.NotNull()).ToArray();

            var methodLevelInterceptorTypes = methodHierarchy
                .SelectMany(m => m.GetCustomAttributes<InterceptorAttribute>())
                .Select(attr => attr.InterceptorType)
                .ToArray();

            return methodLevelInterceptorTypes;
        }
    }
}