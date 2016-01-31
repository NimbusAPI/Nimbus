using Nimbus.Extensions;
using Nimbus.Infrastructure.Logging;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.LoggingTests.TemplateNormalization
{
    [TestFixture]
    public class WhenNormalizingStructuredLogTemplates
    {
        [Test]
        [TestCase("This one has no parameters.", "This one has no parameters.")]
        [TestCase("{0}", "{0}")]
        [TestCase("{0} {1}", "{0} {1}")]
        //[TestCase("{1} {0}", "{1} {0}")]  //FIXME not supported. Don't do this for now.
        [TestCase("A{a}B{b}C{c}", "A{0}B{1}C{2}")]
        [TestCase("A{a}B{b}B{b}", "A{0}B{1}B{1}")]
        public void TheResultingTemplatesShouldBeCompatibleWithStringFormat(string inputTemplate, string expectedTemplate)
        {
            var normalizedTemplate = inputTemplate.NormalizeToStringFormat();
            normalizedTemplate.ShouldBe(expectedTemplate);
        }
    }
}