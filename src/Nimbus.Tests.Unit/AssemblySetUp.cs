using NUnit.Framework;

[assembly: Category("UnitTest")]
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(32)]