using System;
using System.Collections.Generic;
using Nimbus.Configuration;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Transport;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.LargeMessages;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.SqlServer.DeadLetterOffice;
using Nimbus.Transports.SqlServer.DelayedDelivery;
using Nimbus.Transports.SqlServer.MessageSendersAndReceivers;
using Nimbus.Transports.SqlServer.QueueManagement;
using Nimbus.Transports.SqlServer.Schema;

namespace Nimbus.Transports.SqlServer
{
    public class SqlServerTransportConfiguration : TransportConfiguration
    {
        internal string ConnectionString { get; private set; }
        internal TimeSpan PollInterval { get; private set; } = TimeSpan.FromMilliseconds(1000);
        internal bool AutoCreateSchema { get; private set; }

        public SqlServerTransportConfiguration WithConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
            return this;
        }

        public SqlServerTransportConfiguration WithPollInterval(TimeSpan pollInterval)
        {
            PollInterval = pollInterval;
            return this;
        }

        public SqlServerTransportConfiguration WithAutoCreateSchema()
        {
            AutoCreateSchema = true;
            return this;
        }

        protected override void RegisterComponents(PoorMansIoC container)
        {
            container.Register(c => this, ComponentLifetime.SingleInstance);

            container.RegisterType<SqlServerQueueSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<SqlServerQueueReceiver>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<SqlServerTopicSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<SqlServerTopicReceiver>(ComponentLifetime.InstancePerDependency);

            container.RegisterType<SqlServerDelayedDeliveryService>(ComponentLifetime.SingleInstance, typeof(IDelayedDeliveryService));
            container.RegisterType<SqlServerDeadLetterOffice>(ComponentLifetime.SingleInstance, typeof(IDeadLetterOffice));
            container.RegisterType<SqlServerNamespaceCleanser>(ComponentLifetime.SingleInstance, typeof(INamespaceCleanser));
            container.RegisterType<SqlServerSchemaCreator>(ComponentLifetime.SingleInstance);
            container.RegisterType<UnsupportedLargeMessageBodyStore>(ComponentLifetime.SingleInstance, typeof(ILargeMessageBodyStore));

            container.RegisterType<SqlServerTransport>(ComponentLifetime.SingleInstance, typeof(INimbusTransport));
        }

        public override IEnumerable<string> Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
                yield return "A SQL Server connection string must be specified. Use WithConnectionString().";
        }
    }
}
