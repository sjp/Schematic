using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    internal static class SchemaAttributeTests
    {
        [Test]
        public static void Ctor_GivenNullSchema_ThrowsArgumentNullException()
        {
            Assert.That(() => new SchemaAttribute(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptySchema_ThrowsArgumentNullException()
        {
            Assert.That(() => new SchemaAttribute(string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceSchema_ThrowsArgumentNullException()
        {
            Assert.That(() => new SchemaAttribute("   "), Throws.ArgumentNullException);
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
