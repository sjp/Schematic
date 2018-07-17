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
            Assert.Throws<ArgumentOutOfRangeException>(() => new ColumnOrderAttribute(-1));
        }

        [Test]
        public static void ColumnNumber_PropertyGet_MatchesCtorArgument()
        {
            const int columnNumber = 10;
            var columnOrderAttr = new ColumnOrderAttribute(columnNumber);

            Assert.AreEqual(columnNumber, columnOrderAttr.ColumnNumber);
        }
    }
}
