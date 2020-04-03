using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    internal static class SchemaAttributeTests
    {
        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void Ctor_GivenNullOrWhiteSpaceSchema_ThrowsArgumentNullException(string schemaName)
        {
            Assert.That(() => new SchemaAttribute(schemaName), Throws.ArgumentNullException);
        }

        [Test]
        public static void Schema_PropertyGet_MatchesCtorArgument()
        {
            const string schemaValue = "test";
            var schemaAttr = new SchemaAttribute(schemaValue);

            Assert.That(schemaAttr.Schema, Is.EqualTo(schemaValue));
        }
    }
}
