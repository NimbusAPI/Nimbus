using Nimbus.Configuration.Transport;
using Nimbus.Transports.AMQP;
using Nimbus.Transports.InProcess;
using Nimbus.Transports.Nats;
using Nimbus.Transports.Postgres;
using Nimbus.Transports.Redis;
using Nimbus.Transports.SqlServer;

namespace Nimbus.Benchmark;

public static class TransportFactory
{
    public static readonly string[] ValidNames =
    [
        "InProcess", "Redis", "Nats", "NatsJetStream", "Amqp", "SqlServer", "Postgres"
    ];

    public static TransportConfiguration Create(string name) => name.ToLowerInvariant() switch
    {
        "inprocess" => new InProcessTransportConfiguration(),

        "redis" => new RedisTransportConfiguration()
            .WithConnectionString("localhost"),

        "nats" => new NatsTransportConfiguration()
            .WithUrl("nats://localhost:4222")
            .WithCredentials("admin", "password"),

        "natsjetstream" => new NatsTransportConfiguration()
            .WithUrl("nats://localhost:4222")
            .WithCredentials("admin", "password")
            .WithJetStream(),

        "amqp" => new AMQPTransportConfiguration()
            .WithBrokerUri("amqp://localhost:5672")
            .WithCredentials("admin", "admin"),

        "sqlserver" => new SqlServerTransportConfiguration()
            .WithConnectionString("Server=localhost,1433;Database=Nimbus;User Id=sa;Password=Nimbus_Dev_123!;TrustServerCertificate=true;")
            .WithPollInterval(TimeSpan.FromMilliseconds(50))
            .WithAutoCreateSchema(),

        "postgres" => new PostgresTransportConfiguration()
            .WithConnectionString("Host=localhost;Port=5432;Database=nimbus;Username=nimbus;Password=Nimbus_Dev_123!")
            .WithPollInterval(TimeSpan.FromMilliseconds(50))
            .WithAutoCreateSchema(),

        _ => throw new ArgumentException(
            $"Unknown transport '{name}'. Valid options: {string.Join(", ", ValidNames)}")
    };
}
