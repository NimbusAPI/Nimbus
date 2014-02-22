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

    var messageHandlerFactory = new DefaultMessageHandlerFactory(typeProvider);

    var bus = new BusBuilder().Configure()
                                .WithNames("MyTestSuite", Environment.MachineName)
                                .WithConnectionString(CommonResources.ConnectionString)
                                .WithTypesFrom(typeProvider)
                                .WithDefaultHandlerFactory(messageHandlerFactory)
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

### Configuring the bus with Windsor

    //TODO: Set up your own connection string in app.config
    var connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];

    // You'll want a logger. There's a ConsoleLogger and a NullLogger if you really don't care. You can roll your
    // own by implementing the ILogger interface if you want to hook it to an existing logging implementation.
    container.Register(Component.For<ILogger>()
                                .ImplementedBy<SerilogStaticLogger>()
                                .LifestyleSingleton());

    // This is how you tell Nimbus where to find all your message types and handlers.
    var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());

    container.RegisterNimbus(typeProvider);
    container.Register(Component.For<IBus>()
                                .ImplementedBy<Bus>()
                                .UsingFactoryMethod<IBus>(() => new BusBuilder()
                                                                .Configure()
                                                                .WithConnectionString(connectionString)
                                                                .WithNames("PingPong.Windsor", Environment.MachineName)
                                                                .WithTypesFrom(typeProvider)
                                                                .WithWindsorDefaults(container)
                                                                .Build())
                                .LifestyleSingleton()
                                .StartUsingMethod("Start")
        );

### Sending commands

    public class SomeClassThatSendsCommands
    {
        private readonly IBus _bus;
        
        public SomeClassThatSendsCommands(IBus bus)
        {
            _bus = bus;
        }
        
        public async Task SendSomeCommand()
        {
            await _bus.Send(new DoSomethingCommand());
        }
    }

### Handling commands

    public class DoSomethingCommandHandler: IHandleCommand<DoSomethingCommand>
    {
        public async Task Handle(DoSomethingCommand command)
        {
            //TODO: Do something useful here.
        }
    }

### Handling events

    public class ListenForNewOrders : IHandleMulticastEvent<NewOrderRecieved>, IHandleMulticastEvent<PizzaIsReady>
    {
        private readonly IWaitTimeCounter _waitTimeCounter;

        public ListenForNewOrders(IWaitTimeCounter waitTimeCounter)
        {
            _waitTimeCounter = waitTimeCounter;
        }

        public async Task Handle(NewOrderRecieved busEvent)
        {
            Console.WriteLine("I heard about a new order");

            _waitTimeCounter.RecordNewPizzaOrder(busEvent.PizzaId);
        }

        public async Task Handle(PizzaIsReady busEvent)
        {
            Console.WriteLine("I heard about a complete order");

            _waitTimeCounter.RecordPizzaCompleted(busEvent.PizzaId);
        }
    }

### Sending requests

    public static async Task FindOutHowLongItWillTakeToMakeMyPizza()
    {
        var response = await _bus.Request(new HowLongDoPizzasTakeRequest());

        Console.WriteLine("Pizzas take about {0} minutes", response.Minutes);
    }

### Handling requests

    public class HandleWaitTimeRequests : IHandleRequest<HowLongDoPizzasTakeRequest, HowLongDoPizzasTakeResponse>
    {
        private readonly IWaitTimeCounter _waitTimeCounter;

        public HandleWaitTimeRequests(IWaitTimeCounter waitTimeCounter)
        {
            _waitTimeCounter = waitTimeCounter;
        }

        public async Task<HowLongDoPizzasTakeResponse> Handle(HowLongDoPizzasTakeRequest request)
        {
            var currentAverage = _waitTimeCounter.GetAveragePizzaTimes();

            return new HowLongDoPizzasTakeResponse {Minutes = currentAverage};
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

Nimbus is brought to you by Andrew Harcourt [@uglybugger][1] and Damian Maclennan [@damianm][2].

You can follow Nimbus updates on the [NimbusAPI][3] Twitter account.

Are you using Nimbus? We'd love to hear from you. Please get in touch and let us know what you're up to!

[1]: <http://twitter.com/uglybugger>

[2]: <http://twitter.com/damianm>

[3]: <http://twitter.com/NimbusAPI>

[4]: <https://github.com/DamianMac/Nimbus/issues?labels=up-for-grabs&page=1&state=open>
