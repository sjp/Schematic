using System;
using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    internal static class ColumnOrderAttributeTests
    {
        [Test]
        public static void Ctor_GivenNegativeColumnOrder_ThrowsArgumentOutOfRangeException()
        {
            Assert.That(() => new ColumnOrderAttribute(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public static void ColumnNumber_PropertyGet_MatchesCtorArgument()
        {
            const int columnNumber = 10;
            var columnOrderAttr = new ColumnOrderAttribute(columnNumber);

            Assert.That(columnOrderAttr.ColumnNumber, Is.EqualTo(columnNumber));
        }
    }
}
