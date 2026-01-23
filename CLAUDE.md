# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## About Nimbus

Nimbus is a .NET messaging framework that provides an abstraction layer over various messaging transports (Azure Service Bus, Redis, In-Process). It implements message-based communication patterns including commands, events (pub/sub), and request/response messaging.

## Building and Testing

### Prerequisites

Start the development infrastructure (Redis and Seq logging):

```bash
docker-compose up -d
```

This starts:
- Redis server at `localhost:6379`
- Seq logging server at `http://localhost:5341`

### Build Commands

```bash
# Build the solution
dotnet build ./src

# Clean build
dotnet clean ./src
dotnet build ./src

# Restore packages
dotnet restore ./src
```

### Running Tests

```bash
# Run all unit and convention tests
dotnet test

# Run specific test categories via Cake
dotnet cake --Target="ConventionTest"  # Convention tests
dotnet cake --Target="UnitTest"        # Unit tests only
dotnet cake --Target="IntegrationTest" # Integration tests (requires Docker)
dotnet cake --Target="Test"            # Convention + Unit tests

# Run a single test project
dotnet test ./src/Nimbus.Tests.Unit/Nimbus.Tests.Unit.csproj

# Run tests with specific filter
dotnet test --filter "Category=\"UnitTest\""
dotnet test --filter "FullyQualifiedName~WhenDoingSomething"
```

**Test Categories:**
- `Category="Convention"`: Convention/validation tests (naming, interfaces, etc.)
- `Category="UnitTest"`: Fast unit tests with mocks, no external dependencies
- Integration tests: All tests not marked as Convention or UnitTest (require Redis/infrastructure)

**Integration Test Configuration:**
- Tests use `appsettings.json` in `Nimbus.Tests.Integration`
- Override locally with `appsettings.Development.json` (not committed)
- Default points to Docker service names (`redis`, `seq`)

### Build and Publish

```bash
# Full CI build pipeline
dotnet cake --Target="CI"

# Build and test
dotnet cake --Target="BuildAndTest"

# Package creation (NuGet)
dotnet cake --Target="CollectPackages"
```

## Architecture Overview

### Core Design Patterns

Nimbus implements several messaging patterns:

1. **Commands** (`IBusCommand`): Fire-and-forget messages sent to a single handler
2. **Events** (`IBusEvent`): Pub/sub messages with two handler types:
   - `IHandleCompetingEvent<T>`: Competing consumers (load balanced)
   - `IHandleMulticastEvent<T>`: All subscribers receive the message
3. **Requests** (`IBusRequest<TRequest, TResponse>`): Request/response for single handler
4. **Multicast Requests** (`IBusMulticastRequest<TRequest, TResponse>`): Request with multiple responses

### Key Projects

**Core Assemblies:**

- `Nimbus`: Main framework with bus implementation, message pumps, dispatchers, and infrastructure
- `Nimbus.MessageContracts`: Message type marker interfaces (`IBusCommand`, `IBusEvent`, etc.)
- `Nimbus.InfrastructureContracts`: Core abstractions (`IBus`, handler interfaces, `IDependencyResolver`, etc.)

**Transport Implementations:**

- `Nimbus.Transports.AzureServiceBus`: Azure Service Bus transport
- `Nimbus.Transports.Redis`: Redis-based transport using pub/sub and lists
- `Nimbus.Transports.InProcess`: In-memory transport for testing

**Infrastructure:**

- `Nimbus.Containers.Autofac`: Autofac dependency injection integration
- `Nimbus.Serializers.Json`: JSON serialization via Newtonsoft.Json (default is DataContractSerializer)
- `Nimbus.Logger.Serilog`: Serilog integration
- `Nimbus.Logger.Log4net`: Log4net integration
- `Nimbus.LargeMessages.Azure`: Azure Blob Storage for large message payloads

**Examples:**

- `Cafe.*` projects: Sample café application demonstrating message patterns

### Message Flow Architecture

```
User Code
  ↓
IBus → ICommandSender/IEventSender/IRequestSender
  ↓
Router (determines queue/topic path)
  ↓
Transport (INimbusTransport)
  ↓
Queue/Topic

Queue/Topic
  ↓
MessagePump (pulls messages)
  ↓
MessageDispatcher (routes to handlers)
  ↓
Handler (IHandleCommand<T>, IHandleRequest<T,R>, etc.)
```

### Transport Abstraction

All transports implement `INimbusTransport`:

```csharp
internal interface INimbusTransport
{
    INimbusMessageSender GetQueueSender(string queuePath);
    INimbusMessageReceiver GetQueueReceiver(string queuePath);
    INimbusMessageSender GetTopicSender(string topicPath);
    INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName, IFilterCondition filter);
}
```

- `INimbusMessageSender`: Sends `NimbusMessage` envelopes
- `INimbusMessageReceiver`: Receives messages via callback pattern with `Start(Func<NimbusMessage, Task>)`

**NimbusMessage Envelope:**

The transport-agnostic message wrapper containing:
- `MessageId`, `CorrelationId`: Tracking
- `From`, `To`, `DeliverTo`: Routing
- `DeliverAfter`, `ExpiresAfter`: Scheduling/TTL
- `DeliveryAttempts`: Retry tracking
- `Properties`: Metadata dictionary
- `Payload`: The actual message object

### Dependency Injection

Nimbus uses its own DI abstractions to stay container-agnostic:

- `IDependencyResolver`: Top-level resolver
- `IDependencyResolverScope`: Scoped resolver (child scope)

**Scope Management:**
- Each message creates a child scope for handler resolution
- Handlers and their dependencies are resolved per-message
- Singleton services: Bus, senders, routers, serializers

The `Nimbus.Containers.Autofac` project provides Autofac integration via `AutofacDependencyResolver`.

### Routing

- `IRouter`: Routes message types to queue/topic paths
- `IPathFactory`: Generates paths with sanitization and length constraints
- Default: `DestinationPerMessageTypeRouter` creates one queue/topic per message type

### Type Resolution

- `ITypeProvider`: Scans assemblies for message types and handlers
- `AssemblyScanningTypeProvider`: Default implementation
- All known types are provided to serializers for efficient deserialization

### Interceptors

Cross-cutting concerns via interceptor pattern:

- `IInboundInterceptor`: Hooks for incoming messages (before/after/error)
- `IOutboundInterceptor`: Hooks for outgoing messages (before/after/error)
- Priority-based execution order

### Configuration

Bus is configured using a fluent builder:

```csharp
var bus = new BusBuilder()
    .Configure()
    .WithTransport(new RedisTransportConfiguration().WithConnectionString("localhost"))
    .WithNames("MyApp", Environment.MachineName)
    .WithTypesFrom(typeProvider)
    .WithAutofacDefaults(componentContext)
    .WithJsonSerializer()
    .Build();
```

## Working with Tests

### Integration Test Structure

Integration tests use a scenario-based approach:

- Base class: `TestForBus` with Given/When/Then pattern
- Test scenarios in `TestScenarioGeneration/` directory
- `IConfigurationScenario<T>` for composable test configurations
- `AllBusConfigurations<T>` generates combinations of:
  - Transports (InProcess, Redis, Azure Service Bus)
  - Serializers (DataContract, JSON)
  - Routers
  - Large message stores

Example test structure:

```csharp
[TestFixture]
public class WhenDoingSomething : TestForBus
{
    [Test]
    [TestCaseSource(typeof(AllBusConfigurations<WhenDoingSomething>))]
    public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
    {
        await Given(scenario);
        await When();
        // Then methods execute automatically
    }
}
```

### Unit Test Conventions

- Use `SpecificationFor<T>` base classes for BDD-style tests
- No external dependencies (use mocks)
- Tests organized by component (dispatchers, configuration, routing, serialization)

## Common Tasks

### Adding a New Transport

1. Create new project: `Nimbus.Transports.YourTransport`
2. Implement `INimbusTransport` interface
3. Implement `INimbusMessageSender` and `INimbusMessageReceiver`
4. Create configuration class with `ITransportConfiguration`
5. Add test scenarios in `Nimbus.Tests.Integration/TestScenarioGeneration/`

### Adding a New Serializer

1. Create project: `Nimbus.Serializers.YourSerializer`
2. Implement `ISerializer` interface
3. Add extension method on `BusBuilderConfiguration` for fluent config
4. Add to test scenario combinations

### Adding Message Handlers

Handlers implement interfaces from `Nimbus.InfrastructureContracts`:

- `IHandleCommand<TCommand>`
- `IHandleRequest<TRequest, TResponse>`
- `IHandleCompetingEvent<TEvent>`
- `IHandleMulticastEvent<TEvent>`

Property injection interfaces:
- `IRequireBus`: Inject `IBus`
- `IRequireDispatchContext`: Inject dispatch context
- `IRequireNimbusMessage`: Inject raw message envelope

## Project Structure Notes

- Working directory is `src/` (solution root is one level up)
- Solution file: `Nimbus.sln`
- All projects are in `src/` directory
- Build artifacts go to `dist_package/`
- Main branch: `develop` (not `main` or `master`)
