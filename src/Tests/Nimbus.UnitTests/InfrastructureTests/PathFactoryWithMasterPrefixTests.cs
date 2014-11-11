using Nimbus.Infrastructure;
using Nimbus.UnitTests.InfrastructureTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.InfrastructureTests
{
	[TestFixture]
	public class PathFactoryWithMasterPrefixTests
	{
		private const string _prefix = "master";

		[SetUp]
		public void SetUp()
		{
			PathFactory.SetMasterPrefix(_prefix);
		}

		[TearDown]
		public void TearDown()
		{
			PathFactory.SetMasterPrefix("");
		}

		[Test]
		public void WhenCreatingAQueue_WeShouldSeeTheMasterPrefix()
		{
			var pathName = PathFactory.QueuePathFor(typeof(MyCommand<string>));

			const string expected = "master.q.nimbus.unittests.infrastructuretests.messagecontracts.mycommand.1-string";

			pathName.ShouldBe(expected);
		}

		[Test]
		public void WhenCreatingATopic_WeShouldSeeTheMasterPrefix()
		{
			var pathName = PathFactory.TopicPathFor(typeof(MyCommand<string>));

			const string expected = "master.t.nimbus.unittests.infrastructuretests.messagecontracts.mycommand.1-string";

			pathName.ShouldBe(expected);
		}

		[Test]
		public void WhenCreatingASubscriptionForAType_WeShouldSeeTheMasterPrefix()
		{
			var pathName = PathFactory.SubscriptionNameFor("MyLongApplicationName", "Appserver", typeof(MyEventWithALongName));
			pathName.ShouldStartWith(_prefix + ".");
		}
	}
}