# Nimbus

Nimbus is a .NET messaging framework that provides a clean abstraction over multiple messaging transports, supporting commands, events (pub/sub), and request/response patterns.

> **Documentation:** [https://nimbusapi.com](https://nimbusapi.com)

## Supported Transports

| Transport | Package |
|-----------|---------|
| Azure Service Bus | `Nimbus.Transports.AzureServiceBus` |
| Redis | `Nimbus.Transports.Redis` |
| PostgreSQL | `Nimbus.Transports.Postgres` |
| SQL Server | `Nimbus.Transports.SqlServer` |
| AMQP | `Nimbus.Transports.AMQP` |
| In-Process (testing) | `Nimbus.Transports.InProcess` |

## Message Patterns

- **Commands** — fire-and-forget, single handler
- **Events** — pub/sub with competing or multicast consumers
- **Requests** — request/response, single or multicast

## Quick Start

```csharp
var bus = new BusBuilder()
    .Configure()
    .WithTransport(new RedisTransportConfiguration().WithConnectionString("localhost"))
    .WithNames("MyApp", Environment.MachineName)
    .WithTypesFrom(typeProvider)
    .WithAutofacDefaults(componentContext)
    .WithJsonSerializer()
    .Build();

await bus.Start();
```

## Developing Nimbus

```bash
git clone <this repository>
docker-compose up -d --build
dotnet build
```

### Development Infrastructure

`docker-compose up -d --build` starts:

| Service | URL / Port |
|---------|-----------|
| [Seq](https://datalust.co) log server | `http://localhost:5341` |
| [Redis](https://redis.io/) | `localhost:6379` |
| [Apache ActiveMQ Artemis](https://activemq.apache.org/components/artemis/) (AMQP) | `localhost:5672`, console at `http://localhost:8161` |
| SQL Server | `localhost:1433` |
| PostgreSQL | `localhost:5432` |

SQL Server uses a custom Docker image (in `docker/sqlserver`) — the `--build` flag ensures it is built before starting.

Integration tests use `appsettings.json` in the pipeline and `appsettings.Development.json` locally (not committed) to point at localhost ports.

### Sample Application

The `Cafe` sample app demonstrates Nimbus messaging patterns through a simple café scenario (cashier, barista, waiter). Run it with:

```bash
dotnet run --project src/Cafe.Cashier
dotnet run --project src/Cafe.Barista
dotnet run --project src/Cafe.Waiter
```

See [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines.
