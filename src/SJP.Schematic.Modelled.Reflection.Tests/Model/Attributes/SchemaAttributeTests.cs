using System;
using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    internal class SchemaAttributeTests
    {
        [Test]
        public void Ctor_GivenNullSchema_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SchemaAttribute(null));
        }

        [Test]
        public void Ctor_GivenEmptySchema_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SchemaAttribute(string.Empty));
        }

        [Test]
        public void Ctor_GivenWhiteSpaceSchema_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SchemaAttribute("   "));
        }

        [Test]
        public void Schema_PropertyGet_MatchesCtorArgument()
        {
            const string schemaValue = "test";
            var schemaAttr = new SchemaAttribute(schemaValue);

            Assert.AreEqual(schemaValue, schemaAttr.Schema);
        }
    }
}
