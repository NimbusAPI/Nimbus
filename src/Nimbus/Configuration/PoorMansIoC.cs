using System;
using System.Linq;
using System.Reflection;
using Nimbus.ConcurrentCollections;
using Nimbus.Extensions;

namespace Nimbus.Configuration
{
    public class PoorMansIoC : ICreateComponents
    {
        private readonly ThreadSafeDictionary<Type, object> _components = new ThreadSafeDictionary<Type, object>();
        private readonly ThreadSafeDictionary<Type, Func<PoorMansIoC, object>> _factoryDelegates = new ThreadSafeDictionary<Type, Func<PoorMansIoC, object>>();

        private readonly GarbageMan _garbageMan = new GarbageMan();

        public void Register<T>(T instance)
        {
            RegisterInstance(instance);
        }

        public void Register<T>(Func<PoorMansIoC, T> factory)
        {
            RegisterFactoryDelegate(factory);
        }

        private void RegisterFactoryDelegate<T>(Func<PoorMansIoC, T> factory)
        {
            _factoryDelegates[typeof (T)] = pmioc => factory(pmioc);
        }

        private void RegisterInstance(object instance)
        {
            var concreteType = instance.GetType();
            _components[concreteType] = instance;

            var types = concreteType.GetInterfaces();
            foreach (var type in types)
            {
                _components[type] = instance;
            }
        }

        public T Resolve<T>()
        {
            return (T) Resolve(typeof (T));
        }

        private object Resolve(Type type)
        {
            try
            {
                return _components.GetOrAdd(type, ConstructObject);
            }
            catch (Exception exc)
            {
                throw new DependencyResolutionException("Could not resolve tyoe: {0}".FormatWith(type.FullName), exc);
            }
        }

        private object ConstructObject(Type type)
        {
            var instance = ConstructObjectInternal(type);
            var disposable = instance as IDisposable;
            if (disposable != null) _garbageMan.Add(disposable);
            return instance;
        }

        private object ConstructObjectInternal(Type type)
        {
            Func<PoorMansIoC, object> factory;
            if (_factoryDelegates.TryGetValue(type, out factory))
            {
                var instance = factory(this);
                RegisterInstance(instance);
                return instance;
            }
            else
            {
                var concreteType = ExtractConcreteTypeFor(type);
                var args = concreteType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                       .Single()
                                       .GetParameters()
                                       .Select(p => Resolve(p.ParameterType))
                                       .ToArray();

                var instance = Activator.CreateInstance(concreteType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, args, null);
                RegisterInstance(instance);
                return instance;
            }
        }

        private static Type ExtractConcreteTypeFor(Type type)
        {
            var concreteType = typeof (PoorMansIoC).Assembly
                                                   .GetTypes()
                                                   .Where(type.IsAssignableFrom)
                                                   .Where(t => t.IsInstantiable())
                                                   .FirstOrDefault();

            if (concreteType == null) throw new DependencyResolutionException("Could not find a concrete type that implements {0}".FormatWith(type.FullName));

            return concreteType;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _garbageMan.Dispose();
        }
    }
}