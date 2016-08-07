using NUnit.Framework;

[assembly:Category("StressTest")]
[assembly:Category("Slow")]
[assembly: Parallelizable(ParallelScope.None)]  // these are stress tests. We don't want them to be competing for system resources.