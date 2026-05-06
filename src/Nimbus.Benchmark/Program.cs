using System.Diagnostics;
using Autofac;
using Nimbus;
using Nimbus.Benchmark;
using Nimbus.Configuration;
using Nimbus.Configuration.Transport;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Logger.Serilog.Configuration;
using Nimbus.Serializers.Json.Configuration;
using Serilog;

var count = 1000;
var transportName = "InProcess";

for (var i = 0; i < args.Length; i++)
{
    if (args[i] == "--count" && i + 1 < args.Length) count = int.Parse(args[++i]);
    else if (args[i] == "--transport" && i + 1 < args.Length) transportName = args[++i];
}

if (args.Contains("--help") || args.Contains("-h"))
{
    Console.WriteLine("Usage: Nimbus.Benchmark [--transport <name>] [--count <n>]");
    Console.WriteLine($"  Transports: {string.Join(", ", TransportFactory.ValidNames)}");
    Console.WriteLine($"  Default   : --transport InProcess --count 1000");
    return 0;
}

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.Console()
    .CreateLogger();

Console.WriteLine("=== Nimbus Benchmark ===");
Console.WriteLine($"Transport : {transportName}");
Console.WriteLine($"Messages  : {count:N0}");
Console.WriteLine();

TransportConfiguration transportConfig;
try
{
    transportConfig = TransportFactory.Create(transportName);
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

var benchmarkState = new BenchmarkState(count);

var containerBuilder = new ContainerBuilder();
var typeProvider = new AssemblyScanningTypeProvider(typeof(Program).Assembly);
containerBuilder.RegisterNimbus(typeProvider);
containerBuilder.RegisterInstance(benchmarkState).AsSelf().SingleInstance();

containerBuilder.Register(ctx => new BusBuilder()
    .Configure()
    .WithTransport(transportConfig)
    .WithNames("NimbusBenchmark", Environment.MachineName)
    .WithAutofacDefaults(ctx)
    .WithSerilogLogger()
    .WithJsonSerializer()
    .Build())
    .As<IBus>()
    .SingleInstance();

using var container = containerBuilder.Build();
var bus = (Bus)container.Resolve<IBus>();

Console.Write("Starting bus...");
var busReady = new TaskCompletionSource();
bus.Started += (_, _) => busReady.TrySetResult();
await bus.Start();
await busReady.Task;
await Task.Delay(500);
Console.WriteLine(" ready.");
Console.WriteLine();

Console.Write($"Sending {count:N0} messages... ");
var sendStart = Stopwatch.GetTimestamp();
for (var i = 0; i < count; i++)
{
    await bus.Send(new BenchmarkCommand
    {
        SentAtTicks = Stopwatch.GetTimestamp(),
        SequenceNumber = i
    });
}
var sendEnd = Stopwatch.GetTimestamp();
var sendElapsedSec = (sendEnd - sendStart) / (double)Stopwatch.Frequency;
Console.WriteLine($"done in {sendElapsedSec * 1000:F0}ms ({count / sendElapsedSec:F0} msg/s send rate)");

Console.Write("Waiting for all messages... ");
var completed = benchmarkState.WaitForCompletion(TimeSpan.FromMinutes(10));
Console.WriteLine(completed ? "done." : "TIMED OUT!");
Console.WriteLine();

if (completed)
{
    var (lastReceiveTick, latencyTicks) = benchmarkState.GetData();
    var totalElapsedSec = (lastReceiveTick - sendStart) / (double)Stopwatch.Frequency;
    var throughput = count / totalElapsedSec;

    var sortedMs = latencyTicks
        .Select(t => t / (double)Stopwatch.Frequency * 1000.0)
        .OrderBy(x => x)
        .ToArray();

    double P(double pct) => sortedMs[(int)(sortedMs.Length * pct / 100.0)];

    Console.WriteLine("--- Results ---");
    Console.WriteLine($"  Total time   : {totalElapsedSec * 1000:F0}ms");
    Console.WriteLine($"  Throughput   : {throughput:F0} msg/s");
    Console.WriteLine($"  Latency (ms) : min={sortedMs[0]:F2}  p50={P(50):F2}  p95={P(95):F2}  p99={P(99):F2}  max={sortedMs[^1]:F2}");
}

await bus.Stop();
return 0;
