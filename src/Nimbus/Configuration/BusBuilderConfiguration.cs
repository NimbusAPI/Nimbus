﻿using System;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Logger;

namespace Nimbus.Configuration
{
    public class BusBuilderConfiguration
    {
        private readonly BusBuilder _busBuilder;

        internal string InstanceName { get; set; }
        internal string ConnectionString { get; set; }
        internal ICommandBroker CommandBroker { get; set; }
        internal IRequestBroker RequestBroker { get; set; }
        internal IEventBroker EventBroker { get; set; }
        internal Type[] EventHandlerTypes { get; set; }
        internal Type[] CommandHandlerTypes { get; set; }
        internal Type[] RequestHandlerTypes { get; set; }
        internal TimeSpan DefaultTimeout { get; set; }
        internal ILogger Logger { get; set; }

        internal BusBuilderConfiguration(BusBuilder busBuilder)
        {
            _busBuilder = busBuilder;

            DefaultTimeout = TimeSpan.FromSeconds(1);
            Logger = new NullLogger();
        }

        public Bus Build()
        {
            return _busBuilder.Build();
        }
    }
}