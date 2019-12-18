using NUnit.Framework;

[assembly: Category("IntegrationTest")]
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(2)]