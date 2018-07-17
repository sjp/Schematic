using System;
using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    internal static class SchemaAttributeTests
    {
        [Test]
        public static void Ctor_GivenNullSchema_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SchemaAttribute(null));
        }

        [Test]
        public static void Ctor_GivenEmptySchema_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SchemaAttribute(string.Empty));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceSchema_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SchemaAttribute("   "));
        }

        [Test]
        public static void Schema_PropertyGet_MatchesCtorArgument()
        {
            const string schemaValue = "test";
            var schemaAttr = new SchemaAttribute(schemaValue);

            Assert.AreEqual(schemaValue, schemaAttr.Schema);
        }
    }
}
