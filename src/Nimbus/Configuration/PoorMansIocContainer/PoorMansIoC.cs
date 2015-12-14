using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Nimbus.ConcurrentCollections;
using Nimbus.Extensions;

namespace Nimbus.Configuration.PoorMansIocContainer
{
    public class PoorMansIoC : IDisposable
    {
        private readonly ThreadSafeDictionary<Type, object> _singleInstanceComponents = new ThreadSafeDictionary<Type, object>();
        private readonly ThreadSafeDictionary<Type, Func<PoorMansIoC, object>> _factoryDelegates = new ThreadSafeDictionary<Type, Func<PoorMansIoC, object>>();
        private readonly ConcurrentBag<ComponentRegistration> _registrations = new ConcurrentBag<ComponentRegistration>();

        private readonly GarbageMan _garbageMan = new GarbageMan();

        public PoorMansIoC()
        {
            Register(this);
        }

        public void Register<T>(T instance)
        {
            RegisterInstance(instance);
        }

        public void Register<T>(Func<PoorMansIoC, T> factory)
        {
            RegisterFactoryDelegate(factory);
        }

        public void RegisterType<T>(ComponentLifetime lifetime, params Type[] implementedTypes)
        {
            RegisterType(typeof (T), lifetime, implementedTypes);
        }

        public void RegisterType(Type concreteType, ComponentLifetime lifetime, params Type[] implementedTypes)
        {
            if (concreteType.IsInterface) throw new ArgumentException("An interface was supplied where there should have been a concrete type");
            if (implementedTypes.Any(it => !it.IsAssignableFrom(concreteType))) throw new ArgumentException("One or more of the implemented types is not actually implemented by this concrete type.");

            var registrations = implementedTypes.Select(t => new ComponentRegistration(concreteType, t, lifetime)).ToArray();
            if (registrations.None())
            {
                _registrations.Add(new ComponentRegistration(concreteType, concreteType, lifetime));
            }
            else
            {
                foreach (var registration in registrations) _registrations.Add(registration);
            }
        }

        private void RegisterFactoryDelegate<T>(Func<PoorMansIoC, T> factory)
        {
            _factoryDelegates[typeof (T)] = pmioc => factory(pmioc);
        }

        private void RegisterInstance(object instance)
        {
            var concreteType = instance.GetType();
            _singleInstanceComponents[concreteType] = instance;

            var types = concreteType.GetInterfaces();
            foreach (var type in types)
            {
                _singleInstanceComponents[type] = instance;
            }
        }

        public T Resolve<T>()
        {
            return (T) Resolve(typeof (T));
        }

        public T ResolveWithOverrides<T>(params object[] overrides)
        {
            return (T) Resolve(typeof (T), overrides);
        }

        private object Resolve(Type type, params object[] overrides)
        {
            var matchingOverride = overrides
                .Where(type.IsInstanceOfType)
                .FirstOrDefault();
            if (matchingOverride != null) return matchingOverride;

            try
            {
                object instance;
                if (_singleInstanceComponents.TryGetValue(type, out instance)) return instance;
                instance = ConstructObject(type, overrides);
                RegisterInstanceIfSingleton(instance);
                return instance;
            }
            catch (Exception exc)
            {
                throw new DependencyResolutionException("Could not resolve type: {0}".FormatWith(type.FullName), exc);
            }
        }

        private object ConstructObject(Type type, params object[] overrides)
        {
            var instance = ConstructObjectInternal(type, overrides);
            var disposable = instance as IDisposable;
            if (disposable != null) _garbageMan.Add(disposable);
            return instance;
        }

        private object ConstructObjectInternal(Type type, params object[] overrides)
        {
            Func<PoorMansIoC, object> factory;
            if (_factoryDelegates.TryGetValue(type, out factory))
            {
                var instance = factory(this);
                RegisterInstanceIfSingleton(instance);
                return instance;
            }
            else
            {
                var concreteType = ExtractConcreteTypeFor(type);
                var args = concreteType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                       .Single()
                                       .GetParameters()
                                       .Select(p => Resolve(p.ParameterType, overrides))
                                       .ToArray();

                var instance = Activator.CreateInstance(concreteType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, args, null);
                RegisterInstanceIfSingleton(instance);
                return instance;
            }
        }

        private void RegisterInstanceIfSingleton(object instance)
        {
            var componentLifetime = LifetimeFor(instance.GetType());
            if (componentLifetime == ComponentLifetime.SingleInstance)
            {
                RegisterInstance(instance);
            }
        }

        private ComponentLifetime LifetimeFor(Type concreteType)
        {
            var registration = RegistrationForConcreteType(concreteType);

            return registration == null
                ? ComponentLifetime.InstancePerDependency
                : registration.Lifetime;
        }

        private Type ExtractConcreteTypeFor(Type type)
        {
            var registration = RegistrationForImplementedType(type);

            if (registration == null) throw new DependencyResolutionException("No registration for a concrete type that implements {0}".FormatWith(type.FullName));

            return registration.ConcreteType;
        }

        private ComponentRegistration RegistrationForConcreteType(Type type)
        {
            return _registrations
                .Where(r => r.ConcreteType == type)
                .FirstOrDefault();
        }

        private ComponentRegistration RegistrationForImplementedType(Type type)
        {
            return _registrations
                .Where(r => r.ImplementedType == type)
                .FirstOrDefault();
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