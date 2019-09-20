using System;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Dbml.Tests
{
    internal static class DbmlFormatterTests
    {
        [Test]
        public static void RenderTables_GivenNullTables_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DbmlFormatter().RenderTables(null));
        }

        [Test]
        public static void RenderTables_GivenEmptyTables_ReturnsEmptyString()
        {
            var formatter = new DbmlFormatter();
            var tables = Array.Empty<IRelationalDatabaseTable>();

            var result = formatter.RenderTables(tables);

            Assert.AreEqual(string.Empty, result);
        }
    }
}
