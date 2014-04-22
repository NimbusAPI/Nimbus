using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;

namespace Nimbus.Interceptors
{
    internal class InterceptorFactory : IInterceptorFactory
    {
        private readonly GlobalInterceptorTypesSetting _globalInterceptorTypes;

        public InterceptorFactory(GlobalInterceptorTypesSetting globalInterceptorTypes)
        {
            _globalInterceptorTypes = globalInterceptorTypes;
        }

        public IMessageInterceptor[] CreateInterceptors(IDependencyResolverScope scope, object handler, object message)
        {
            var globalInterceptors = GetGlobalInterceptorTypes();
            var classLevelInterceptors = GetClassLevelInterceptorTypes(handler);
            var methodLevelInterceptors = GetMethodLevelInterceptorTypes(scope, handler);

            var interceptors = new Type[0]
                .Union(globalInterceptors)
                .Union(classLevelInterceptors)
                .Union(methodLevelInterceptors)
                .DistinctBy(t => t.FullName)
                .Select(t => (IMessageInterceptor) scope.Resolve(t, t.FullName))
                .OrderByDescending(i => i.Priority)
                .ThenBy(i => i.GetType().FullName)
                .ToArray();

            return interceptors;
        }

        private IEnumerable<Type> GetGlobalInterceptorTypes()
        {
            var globalInterceptors = _globalInterceptorTypes.Value
                                                            .ToArray();
            return globalInterceptors;
        }

        private static IEnumerable<Type> GetClassLevelInterceptorTypes(object handler)
        {
            var classHierarchy = new[] {handler.GetType()}.DepthFirst(t => new[] {t.BaseType}.NotNull()).ToArray();

            var classLevelInterceptors = classHierarchy
                .SelectMany(t => t.GetCustomAttributes<InterceptorAttribute>())
                .Select(attr => attr.InterceptorType)
                .ToArray();

            return classLevelInterceptors;
        }

        private IEnumerable<Type> GetMethodLevelInterceptorTypes(IDependencyResolverScope scope, object handler)
        {
            //FIXME it might be nice to implement this ;)
            yield break;
        }
    }
}