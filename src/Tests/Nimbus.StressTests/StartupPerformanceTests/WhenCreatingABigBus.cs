using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using ConfigInjector.QuickAndDirty;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Logging;
using Nimbus.MessageContracts;
using Nimbus.Tests.Common.Configuration;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Transports.WindowsServiceBus;
using Nimbus.Transports.WindowsServiceBus.QueueManagement;
using NUnit.Framework;

namespace Nimbus.StressTests.StartupPerformanceTests
{
    [TestFixture]
    [Explicit("We pay $$ for messages when we're hitting the Azure Message Bus. Let's not run these on CI builds.")]
    public class WhenCreatingABigBus
    {
        private NamespaceCleanser _namespaceCleanser;
        private GlobalPrefixSetting _globalPrefix;

        [SetUp]
        public void SetUp()
        {
            _globalPrefix = new GlobalPrefixSetting {Value = Guid.NewGuid().ToString()};
            _namespaceCleanser = new NamespaceCleanser(new ConnectionStringSetting {Value = DefaultSettingsReader.Get<AzureServiceBusConnectionString>()},
                                                       _globalPrefix,
                                                       new NullLogger());
            _namespaceCleanser.RemoveAllExistingNamespaceElements().Wait();
        }

        [TearDown]
        public void TearDown()
        {
            _namespaceCleanser.RemoveAllExistingNamespaceElements().Wait();
        }

        [Test]
        [Timeout(10*60*1000)]
        public async Task TheStartupTimeShouldBeAcceptable()
        {
            const int numMessageTypes = 50;

            var assemblyBuilder = EmitMessageContractsAndHandlersAssembly(numMessageTypes);

            var logger = TestHarnessLoggerFactory.Create();
            var typeProvider = new AssemblyScanningTypeProvider(assemblyBuilder);

            var firstBus = new BusBuilder().Configure()
                                           .WithNames("MyTestSuite", Environment.MachineName)
                                           .WithDefaults(typeProvider)
                                           .WithTransport(new WindowsServiceBusTransportConfiguration()
                                                              .WithConnectionString(DefaultSettingsReader.Get<AzureServiceBusConnectionString>()))
                                           .WithLogger(logger)
                                           .WithDebugOptions(
                                               dc =>
                                                   dc.RemoveAllExistingNamespaceElementsOnStartup(
                                                       "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                           .Build();
            try
            {
                try
                {
                    await firstBus.Start(MessagePumpTypes.All);
                }
                catch (AggregateException exc)
                {
                    throw exc.Flatten();
                }
            }
            finally
            {
                firstBus.Dispose();
            }

            var subsequentBus = new BusBuilder().Configure()
                                                .WithNames("MyTestSuite", Environment.MachineName)
                                                .WithTransport(new WindowsServiceBusTransportConfiguration()
                                                                   .WithConnectionString(DefaultSettingsReader.Get<AzureServiceBusConnectionString>()))
                                                .WithDefaults(typeProvider)
                                                .WithLogger(logger)
                                                .Build();

            try
            {
                try
                {
                    await subsequentBus.Start(MessagePumpTypes.All);
                }
                catch (AggregateException exc)
                {
                    throw exc.Flatten();
                }
            }
            finally
            {
                subsequentBus.Dispose();
            }
        }

        private static Assembly EmitMessageContractsAndHandlersAssembly(int numMessageTypes)
        {
            var assemblyName = new AssemblyName("NimbusStartupPerformanceTests");
            var appDomain = Thread.GetDomain();
            var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            for (var i = 0; i < numMessageTypes; i++)
            {
                var busCommandType = EmitBusCommandType(i, moduleBuilder);
                var busCommandHandlerType = EmitBusCommandHandlerType(i, moduleBuilder, busCommandType);

                var busEventType = EmitBusEventType(i, moduleBuilder);
                var busMulticastEventHandlerType = EmitBusMulticastEventHandlerType(i, moduleBuilder, busEventType);
                var busCompetingEventHandlerType = EmitBusCompetingEventHandlerType(i, moduleBuilder, busEventType);
            }

            return assemblyBuilder;
        }

        private static Type EmitBusCommandType(int i, ModuleBuilder moduleBuilder)
        {
            var commandTypeName = "LotsOfCommands{0:00}".FormatWith(i);
            var busCommandTypeBuilder = moduleBuilder.DefineType(commandTypeName,
                                                                 TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                                                                 TypeAttributes.BeforeFieldInit,
                                                                 typeof(Object));
            busCommandTypeBuilder.AddInterfaceImplementation(typeof(IBusCommand));
            var busCommandType = busCommandTypeBuilder.CreateType();
            return busCommandType;
        }

        private static Type EmitBusCommandHandlerType(int i, ModuleBuilder moduleBuilder, Type busCommandType)
        {
            var commandHandlerTypeName = "LotsOfCommandsHandler{0:00}".FormatWith(i);
            var busCommandHandlerTypeBuilder = moduleBuilder.DefineType(commandHandlerTypeName,
                                                                        TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                                                                        TypeAttributes.BeforeFieldInit,
                                                                        typeof(Object));
            var genericHandlerType = typeof(IHandleCommand<>).MakeGenericType(busCommandType);
            busCommandHandlerTypeBuilder.AddInterfaceImplementation(genericHandlerType);

            var methodBuilder = busCommandHandlerTypeBuilder.DefineMethod("Handle", MethodAttributes.Public | MethodAttributes.Virtual, typeof(Task), new[] {busCommandType});
            var il = methodBuilder.GetILGenerator();
            il.ThrowException(typeof(NotImplementedException));

            var handleMethod = genericHandlerType.GetMethod("Handle");

            busCommandHandlerTypeBuilder.DefineMethodOverride(methodBuilder, handleMethod);

            return busCommandHandlerTypeBuilder.CreateType();
        }

        private static Type EmitBusEventType(int i, ModuleBuilder moduleBuilder)
        {
            var eventTypeName = "LotsOfEvents{0:00}".FormatWith(i);
            var busEventTypeBuilder = moduleBuilder.DefineType(eventTypeName,
                                                               TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                                                               TypeAttributes.BeforeFieldInit,
                                                               typeof(Object));
            busEventTypeBuilder.AddInterfaceImplementation(typeof(IBusEvent));
            var busEventType = busEventTypeBuilder.CreateType();
            return busEventType;
        }

        private static Type EmitBusMulticastEventHandlerType(int i, ModuleBuilder moduleBuilder, Type busEventType)
        {
            var multicastEventHandlerTypeName = "LotsOfMulticastEventsHandler{0:00}".FormatWith(i);
            var busMulticastEventHandlerTypeBuilder = moduleBuilder.DefineType(multicastEventHandlerTypeName,
                                                                               TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                                                                               TypeAttributes.BeforeFieldInit,
                                                                               typeof(Object));
            var genericHandlerType = typeof(IHandleMulticastEvent<>).MakeGenericType(busEventType);
            busMulticastEventHandlerTypeBuilder.AddInterfaceImplementation(genericHandlerType);

            var methodBuilder = busMulticastEventHandlerTypeBuilder.DefineMethod("Handle", MethodAttributes.Public | MethodAttributes.Virtual, typeof(Task), new[] {busEventType});
            var il = methodBuilder.GetILGenerator();
            il.ThrowException(typeof(NotImplementedException));

            var handleMethod = genericHandlerType.GetMethod("Handle");

            busMulticastEventHandlerTypeBuilder.DefineMethodOverride(methodBuilder, handleMethod);

            return busMulticastEventHandlerTypeBuilder.CreateType();
        }

        private static Type EmitBusCompetingEventHandlerType(int i, ModuleBuilder moduleBuilder, Type busEventType)
        {
            var competingEventHandlerTypeName = "LotsOfCompetingEventsHandler{0:00}".FormatWith(i);
            var busCompetingEventHandlerTypeBuilder = moduleBuilder.DefineType(competingEventHandlerTypeName,
                                                                               TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                                                                               TypeAttributes.BeforeFieldInit,
                                                                               typeof(Object));
            var genericHandlerType = typeof(IHandleCompetingEvent<>).MakeGenericType(busEventType);
            busCompetingEventHandlerTypeBuilder.AddInterfaceImplementation(genericHandlerType);

            var methodBuilder = busCompetingEventHandlerTypeBuilder.DefineMethod("Handle", MethodAttributes.Public | MethodAttributes.Virtual, typeof(Task), new[] {busEventType});
            var il = methodBuilder.GetILGenerator();
            il.ThrowException(typeof(NotImplementedException));

            var handleMethod = genericHandlerType.GetMethod("Handle");

            busCompetingEventHandlerTypeBuilder.DefineMethodOverride(methodBuilder, handleMethod);

            return busCompetingEventHandlerTypeBuilder.CreateType();
        }
    }
}