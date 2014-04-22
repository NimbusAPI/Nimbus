using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;

namespace Nimbus.Interceptors.Inbound
{
    internal class InboundInterceptorFactory : IInboundInterceptorFactory
    {
        private readonly GlobalInterceptorTypesSetting _globalInterceptorTypes;

        public InboundInterceptorFactory(GlobalInterceptorTypesSetting globalInterceptorTypes)
        {
            _globalInterceptorTypes = globalInterceptorTypes;
        }

        public IInboundInterceptor[] CreateInterceptors(IDependencyResolverScope scope, object handler, object message)
        {
            var globalInterceptors = GetGlobalInterceptorTypes();
            var classLevelInterceptors = GetClassLevelInterceptorTypes(handler);
            var methodLevelInterceptors = GetMethodLevelInterceptorTypes(handler, message);

            var interceptors = new Type[0]
                .Union(globalInterceptors)
                .Union(classLevelInterceptors)
                .Union(methodLevelInterceptors)
                .DistinctBy(t => t.FullName)
                .Select(t => (IInboundInterceptor) scope.Resolve(t, t.FullName))
                .OrderByDescending(i => i.Priority)
                .ThenBy(i => i.GetType().FullName)
                .ToArray();

            return interceptors;
        }

        private IEnumerable<Type> GetGlobalInterceptorTypes()
        {
            var globalInterceptorTypes = _globalInterceptorTypes.Value
                                                                .ToArray();
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

            var method = handler.GetType().GetMethods()
                                .Where(m => string.Compare(m.Name, "Handle", StringComparison.OrdinalIgnoreCase) == 0)
                                .Where(m => m.GetParameters().Count() == 1)
                                .Where(m => m.GetParameters().First().ParameterType == messageType)
                                .Single();

            var methodHierarchy = new[] {method}.DepthFirst(m => new[] {m.GetBaseDefinition()}.NotNull()).ToArray();

            var methodLevelInterceptorTypes = methodHierarchy
                .SelectMany(m => m.GetCustomAttributes<InterceptorAttribute>())
                .Select(attr => attr.InterceptorType)
                .ToArray();

            return methodLevelInterceptorTypes;
        }
    }
}