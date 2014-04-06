using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Infrastructure;
using Nimbus.Logger;
using Nimbus.MessageContracts;
using NUnit.Framework;

namespace Nimbus.IntegrationTests.Tests.StartupPerformanceTests
{
    [TestFixture]
    public class WhenCreatingABigBus
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        [Timeout(10*60*1000)]
        public async Task TheStartupTimeShouldBeAcceptable()
        {
            const int numMessageTypes = 25;

            var assemblyBuilder = EmitMessageContractsAndHandlersAssembly(numMessageTypes);

            var logger = new ConsoleLogger();
            var typeProvider = new AssemblyScanningTypeProvider(new[] {assemblyBuilder});
            var messageHandlerFactory = new DefaultMessageHandlerFactory(typeProvider);

            using (new AssertingStopwatch("First bus creation", TimeSpan.FromMinutes(5)))
            {
                using (var bus = new BusBuilder().Configure()
                                                 .WithNames("MyTestSuite", Environment.MachineName)
                                                 .WithConnectionString(CommonResources.ConnectionString)
                                                 .WithTypesFrom(typeProvider)
                                                 .WithDefaultHandlerFactory(messageHandlerFactory)
                                                 .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                                 .WithLogger(logger)
                                                 .WithDebugOptions(
                                                     dc =>
                                                         dc.RemoveAllExistingNamespaceElementsOnStartup(
                                                             "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                                 .Build())
                {
                    try
                    {
                        bus.Start();
                    }
                    catch (AggregateException exc)
                    {
                        throw exc.Flatten();
                    }
                }
            }

            for (var i = 0; i < 10; i++)
            {
                Console.WriteLine();
            }

            using (new AssertingStopwatch("Subsequent bus creation", TimeSpan.FromSeconds(20)))
            {
                using (var bus = new BusBuilder().Configure()
                                                 .WithNames("MyTestSuite", Environment.MachineName)
                                                 .WithConnectionString(CommonResources.ConnectionString)
                                                 .WithTypesFrom(typeProvider)
                                                 .WithDefaultHandlerFactory(messageHandlerFactory)
                                                 .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                                 .WithLogger(logger)
                                                 .Build())
                {
                    try
                    {
                        bus.Start();
                    }
                    catch (AggregateException exc)
                    {
                        throw exc.Flatten();
                    }
                }
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
                                                                 typeof (Object));
            busCommandTypeBuilder.AddInterfaceImplementation(typeof (IBusCommand));
            var busCommandType = busCommandTypeBuilder.CreateType();
            return busCommandType;
        }

        private static Type EmitBusCommandHandlerType(int i, ModuleBuilder moduleBuilder, Type busCommandType)
        {
            var commandHandlerTypeName = "LotsOfCommandsHandler{0:00}".FormatWith(i);
            var busCommandHandlerTypeBuilder = moduleBuilder.DefineType(commandHandlerTypeName,
                                                                        TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                                                                        TypeAttributes.BeforeFieldInit,
                                                                        typeof (Object));
            var genericHandlerType = typeof (IHandleCommand<>).MakeGenericType(busCommandType);
            busCommandHandlerTypeBuilder.AddInterfaceImplementation(genericHandlerType);

            var methodBuilder = busCommandHandlerTypeBuilder.DefineMethod("Handle", MethodAttributes.Public | MethodAttributes.Virtual, typeof (Task), new[] {busCommandType});
            var il = methodBuilder.GetILGenerator();
            il.ThrowException(typeof (NotImplementedException));

            var handleMethod = genericHandlerType.GetMethod("Handle");

            busCommandHandlerTypeBuilder.DefineMethodOverride(methodBuilder, handleMethod);

            return busCommandHandlerTypeBuilder.CreateType();
        }

        private static Type EmitBusEventType(int i, ModuleBuilder moduleBuilder)
        {
            var EventTypeName = "LotsOfEvents{0:00}".FormatWith(i);
            var busEventTypeBuilder = moduleBuilder.DefineType(EventTypeName,
                                                               TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                                                               TypeAttributes.BeforeFieldInit,
                                                               typeof (Object));
            busEventTypeBuilder.AddInterfaceImplementation(typeof (IBusEvent));
            var busEventType = busEventTypeBuilder.CreateType();
            return busEventType;
        }

        private static Type EmitBusMulticastEventHandlerType(int i, ModuleBuilder moduleBuilder, Type busEventType)
        {
            var MulticastEventHandlerTypeName = "LotsOfMulticastEventsHandler{0:00}".FormatWith(i);
            var busMulticastEventHandlerTypeBuilder = moduleBuilder.DefineType(MulticastEventHandlerTypeName,
                                                                               TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                                                                               TypeAttributes.BeforeFieldInit,
                                                                               typeof (Object));
            var genericHandlerType = typeof (IHandleMulticastEvent<>).MakeGenericType(busEventType);
            busMulticastEventHandlerTypeBuilder.AddInterfaceImplementation(genericHandlerType);

            var methodBuilder = busMulticastEventHandlerTypeBuilder.DefineMethod("Handle", MethodAttributes.Public | MethodAttributes.Virtual, typeof (Task), new[] {busEventType});
            var il = methodBuilder.GetILGenerator();
            il.ThrowException(typeof (NotImplementedException));

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
                                                                               typeof (Object));
            var genericHandlerType = typeof (IHandleCompetingEvent<>).MakeGenericType(busEventType);
            busCompetingEventHandlerTypeBuilder.AddInterfaceImplementation(genericHandlerType);

            var methodBuilder = busCompetingEventHandlerTypeBuilder.DefineMethod("Handle", MethodAttributes.Public | MethodAttributes.Virtual, typeof (Task), new[] {busEventType});
            var il = methodBuilder.GetILGenerator();
            il.ThrowException(typeof (NotImplementedException));

            var handleMethod = genericHandlerType.GetMethod("Handle");

            busCompetingEventHandlerTypeBuilder.DefineMethodOverride(methodBuilder, handleMethod);

            return busCompetingEventHandlerTypeBuilder.CreateType();
        }
    }
}