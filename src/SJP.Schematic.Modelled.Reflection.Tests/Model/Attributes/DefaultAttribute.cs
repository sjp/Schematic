using System.Globalization;
using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    internal static class DefaultAttributeTests
    {
        [Test]
        public static void Ctor_GivenNullStringValue_ThrowsArgumentNullException()
        {
            Assert.That(() => new DefaultAttribute(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void DefaultValue_PropertyGetFromStringCtorArg_MatchesCtorArgument()
        {
            const string defaultValue = "test";
            var defaultAttr = new DefaultAttribute(defaultValue);

            Assert.That(defaultAttr.DefaultValue, Is.EqualTo(defaultValue));
        }

        [Test]
        public static void DefaultValue_PropertyGetFromDoubleCtorArg_MatchesCtorArgument()
        {
            const double defaultValue = 3.1415926;
            var defaultAttr = new DefaultAttribute(defaultValue);

            Assert.That(defaultAttr.DefaultValue, Is.EqualTo(defaultValue.ToString(CultureInfo.InvariantCulture)));
        }

        [Test]
        public static void DefaultValue_PropertyGetFromIntCtorArg_MatchesCtorArgument()
        {
            const int defaultValue = 100;
            var defaultAttr = new DefaultAttribute(defaultValue);

            Assert.That(defaultAttr.DefaultValue, Is.EqualTo(defaultValue.ToString(CultureInfo.InvariantCulture)));
        }
    }
}
