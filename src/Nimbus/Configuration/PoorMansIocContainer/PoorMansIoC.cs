using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Nimbus.Extensions;

namespace Nimbus.Configuration.PoorMansIocContainer
{
    public class PoorMansIoC : IDisposable
    {
        private readonly ConcurrentBag<object> _singleInstanceComponents = new ConcurrentBag<object>();
        private readonly ConcurrentBag<IComponentRegistration> _registrations = new ConcurrentBag<IComponentRegistration>();

        private readonly GarbageMan _garbageMan = new GarbageMan();

        public PoorMansIoC()
        {
            Register(this);
        }

        public void Register<T>(T instance, params Type[] implementedTypes)
        {
            var advertisedTypes = implementedTypes.None() ? new[] {instance.GetType()} : implementedTypes;
            RegisterInstance(instance, advertisedTypes);
        }

        public void Register<T>(Func<PoorMansIoC, T> factory, ComponentLifetime componentLifetime, params Type[] implementedTypes)
        {
            foreach (var t in implementedTypes)
            {
                if (!t.IsAssignableFrom(typeof (T))) throw new ArgumentException("Factory return type {0} is not assignable to {1}".FormatWith(typeof (T).FullName, t.FullName));
            }

            RegisterFactoryDelegate(factory, componentLifetime, implementedTypes);
        }

        public void RegisterType<T>(ComponentLifetime lifetime, params Type[] implementedTypes)
        {
            foreach (var t in implementedTypes)
            {
                if (!t.IsAssignableFrom(typeof (T))) throw new ArgumentException("Concrete type {0} is not assignable to {1}".FormatWith(typeof (T).FullName, t.FullName));
            }

            RegisterType(typeof (T), lifetime, implementedTypes);
        }

        public void RegisterType(Type concreteType, ComponentLifetime lifetime, params Type[] implementedTypes)
        {
            if (concreteType.IsInterface) throw new ArgumentException("An interface was supplied where there should have been a concrete type");
            if (implementedTypes.Any(it => !it.IsAssignableFrom(concreteType)))
                throw new ArgumentException("One or more of the implemented types is not actually implemented by this concrete type.");

            var advertisedTypes = implementedTypes.None() ? new[] {concreteType} : implementedTypes;

            advertisedTypes.Select(t => new TypedRegistration(concreteType, t, lifetime))
                           .Do(r => _registrations.Add(r))
                           .Done();
        }

        private void RegisterFactoryDelegate<T>(Func<PoorMansIoC, T> factory, ComponentLifetime componentLifetime, Type[] implementedTypes)
        {
            var providedTypes = implementedTypes.None() ? new[] {typeof (T)} : implementedTypes;

            foreach (var type in providedTypes)
            {
                _registrations.Add(new FactoryRegistration(c => factory(c), typeof (T), componentLifetime, type));
            }
        }

        private void RegisterInstance(object instance, Type[] implementedTypes)
        {
            foreach (var implementedType in implementedTypes)
            {
                _registrations.Add(new TypedRegistration(instance.GetType(), implementedType, ComponentLifetime.SingleInstance));
            }
            _singleInstanceComponents.Add(instance);
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

            var registration = RegistrationForImplementedType(type);
            if (registration == null)
            {
                throw new DependencyResolutionException("No registration for type")
                    .WithData("RequestedType", type.FullName);
            }

            try
            {
                var concreteType = registration.ConcreteType;

                var instance = _singleInstanceComponents
                    .Where(o => o.GetType() == concreteType)
                    .FirstOrDefault();
                if (instance != null) return instance;

                instance = ConstructObject(type, registration.Lifetime, overrides);
                return instance;
            }
            catch (Exception exc)
            {
                throw new DependencyResolutionException("Failed to construct type", exc)
                    .WithData("RequestedType", type.FullName);
            }
        }

        private object ConstructObject(Type type, ComponentLifetime componentLifetime, params object[] overrides)
        {
            var registration = _registrations
                .Where(r => r.ImplementedType == type)
                .First();

            object instance = null;

            var factoryRegistration = registration as FactoryRegistration;
            if (factoryRegistration != null)
            {
                instance = factoryRegistration.Factory(this);
            }

            var typedRegistration = registration as TypedRegistration;
            if (typedRegistration != null)
            {
                var constructorInfo = typedRegistration.ConcreteType
                                                       .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                                       .Single();
                var args = constructorInfo
                    .GetParameters()
                    .Select(p => Resolve(p.ParameterType, overrides))
                    .ToArray();

                instance = constructorInfo.Invoke(args);
            }

            if (instance == null) throw new DependencyResolutionException("No registration was found for type {0}".FormatWith(type.FullName));

            if (componentLifetime == ComponentLifetime.SingleInstance)
            {
                _singleInstanceComponents.Add(instance);
            }

            var disposable = instance as IDisposable;
            if (disposable != null) _garbageMan.Add(disposable);

            return instance;
        }

        private IComponentRegistration RegistrationForImplementedType(Type type)
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