# Nimbus
Nimbus is a .NET client library to add an easy to develop against experience against the Azure Service Bus.

If you've used NServiceBus or MassTransit before, you'll be right at home.

Nimbus was designed to be lightweight and pluggable. You won't find fifty conflicting versions of other projects ILMerged into the binary. If you want to plug your own container or logging framework in, go right ahead. If you
want something that will just work out of the box we give you that, too.

Nimbus provides implementations of all of the common messaging patterns for building distributed, service-oriented systems.

## How to get started?
### Grabbing the packages
It's on NuGet:

    Install-Package Nimbus

If you're using a bus you should probably be using an IoC container. If you like Autofac, grab the corresponding bundle:

    Install-Package Nimbus.Autofac

### Configuring the bus without a container

    // This is how you tell Nimbus where to find all your message types and handlers.
    var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());

    var messageBroker = new DefaultMessageBroker(typeProvider);

    var bus = new BusBuilder().Configure()
                                .WithNames("MyTestSuite", Environment.MachineName)
                                .WithConnectionString(CommonResources.ConnectionString)
                                .WithTypesFrom(typeProvider)
                                .WithCommandBroker(messageBroker)
                                .WithRequestBroker(messageBroker)
                                .WithMulticastEventBroker(messageBroker)
                                .WithCompetingEventBroker(messageBroker)
                                .WithMulticastRequestBroker(messageBroker)
                                .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                .Build();
    bus.Start();
    return bus;

### Configuring the bus with Autofac
    //TODO: Set up your own connection string in app.config
    var connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];

    // You'll want a logger. There's a ConsoleLogger and a NullLogger if you really don't care. You can roll your
	// own by implementing the ILogger interface if you want to hook it to an existing logging implementation.
    builder.RegisterType<ConsoleLogger>()
           .AsImplementedInterfaces()
           .SingleInstance();

    // This is how you tell Nimbus where to find all your message types and handlers.
    var handlerTypesProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());
    
    builder.RegisterNimbus(handlerTypesProvider);
    builder.Register(componentContext => new BusBuilder()
                         .Configure()
                         .WithConnectionString(connectionString)
                         .WithNames("MyApp", Environment.MachineName)
                         .WithTypesFrom(handlerTypesProvider)
                         .WithAutofacDefaults(componentContext)
                         .Build())
           .As<IBus>()
           .AutoActivate()
           .OnActivated(c => c.Instance.Start())
           .SingleInstance();

    var container = builder.Build();
    return container;
    
### Sending commands

    public class SomeClassThatSendsCommands
    {
        private readonly IBus _bus;
        
        public SomeClassThatSendsCommands(IBus bus)
        {
            _bus = bus;
        }
        
        public void SendSomeCommand()
        {
            _bus.Send(new DoSomethingCommand());
        }
    }

### Handling commands

    public class DoSomethingCommandHandler: IHandleCommand<DoSomethingCommand>
    {
        public void Handle(DoSomethingCommand command)
        {
            //TODO: Do something useful here.
        }
    }

## Can I contribute?
Absolutely! This is very very very early days for this project. We need things
like:

1.  Documentation
1.  More container implementations
1.  Logger implementations
1.  Samples
1.  Real world input from your projects

We've put some issues here marked as [Up-for-grabs][4] if you want to jump in.

[4]: <https://github.com/DamianMac/Nimbus/issues?labels=up-for-grabs&page=1&state=open>

Nimbus is brought to you by Andrew Harcourt [@uglybugger][1] and Damian Maclennan [@damianm][2].

[1]: <http://twitter.com/uglybugger>

[2]: <http://twitter.com/damianm>

You can follow Nimbus updates on the [NimbusAPI][3] Twitter account.

[3]: <http://twitter.com/NimbusAPI>
