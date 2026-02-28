using System;
using System.Collections.Generic;
using Nimbus.Configuration;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Transport;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.LargeMessages;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Postgres.DeadLetterOffice;
using Nimbus.Transports.Postgres.DelayedDelivery;
using Nimbus.Transports.Postgres.MessageSendersAndReceivers;
using Nimbus.Transports.Postgres.QueueManagement;
using Nimbus.Transports.Postgres.Schema;

namespace Nimbus.Transports.Postgres
{
    public class PostgresTransportConfiguration : TransportConfiguration
    {
        internal string ConnectionString { get; private set; }
        internal TimeSpan PollInterval { get; private set; } = TimeSpan.FromMilliseconds(1000);
        internal bool AutoCreateSchema { get; private set; }

        public PostgresTransportConfiguration WithConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
            return this;
        }

        public PostgresTransportConfiguration WithPollInterval(TimeSpan pollInterval)
        {
            PollInterval = pollInterval;
            return this;
        }

        public PostgresTransportConfiguration WithAutoCreateSchema()
        {
            AutoCreateSchema = true;
            return this;
        }

        protected override void RegisterComponents(PoorMansIoC container)
        {
            container.Register(c => this, ComponentLifetime.SingleInstance);

            container.RegisterType<PostgresQueueSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<PostgresQueueReceiver>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<PostgresTopicSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<PostgresTopicReceiver>(ComponentLifetime.InstancePerDependency);

            container.RegisterType<PostgresDelayedDeliveryService>(ComponentLifetime.SingleInstance, typeof(IDelayedDeliveryService));
            container.RegisterType<PostgresDeadLetterOffice>(ComponentLifetime.SingleInstance, typeof(IDeadLetterOffice));
            container.RegisterType<PostgresNamespaceCleanser>(ComponentLifetime.SingleInstance, typeof(INamespaceCleanser));
            container.RegisterType<PostgresSchemaCreator>(ComponentLifetime.SingleInstance);
            container.RegisterType<UnsupportedLargeMessageBodyStore>(ComponentLifetime.SingleInstance, typeof(ILargeMessageBodyStore));

            container.RegisterType<PostgresTransport>(ComponentLifetime.SingleInstance, typeof(INimbusTransport));
        }

        public override IEnumerable<string> Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
                yield return "A PostgreSQL connection string must be specified. Use WithConnectionString().";
        }
    }
}
