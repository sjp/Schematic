using System;
using System.Collections.Generic;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Dot.Tests
{
    [TestFixture]
    internal static class DotFormatterTests
    {
        private static IIdentifierDefaults IdentifierDefaults { get; } = new IdentifierDefaultsBuilder()
            .WithServer("server")
            .WithDatabase("database")
            .WithSchema("schema")
            .Build();

        private static IDotFormatter Formatter { get; } = new DotFormatter(IdentifierDefaults);

        [Test]
        public static void Ctor_GivenNullDefaults_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DotFormatter(null));
        }

        [Test]
        public static void RenderTables_GivenNullTables_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Formatter.RenderTables(null));
        }

        [Test]
        public static void Ctor_GivenNullTablesWithValidOptions_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Formatter.RenderTables(null, new DotRenderOptions()));
        }

        [Test]
        public static void Ctor_GivenNullOptionsWithValidTables_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Formatter.RenderTables(Array.Empty<IRelationalDatabaseTable>(), (DotRenderOptions)null));
        }

        [Test]
        public static void Ctor_GivenNullTablesWithValidRowCounts_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Formatter.RenderTables(null, new Dictionary<Identifier, ulong>()));
        }

        [Test]
        public static void Ctor_GivenNullTablesWithValidRowCountsAndOptions_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Formatter.RenderTables(null, new Dictionary<Identifier, ulong>(), new DotRenderOptions()));
        }

        [Test]
        public static void Ctor_GivenNullRowCountsWithValidTablesAndOptions_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Formatter.RenderTables(Array.Empty<IRelationalDatabaseTable>(), null, new DotRenderOptions()));
        }

        [Test]
        public static void Ctor_GivenNullOptionsWithTablesAndValidRowCounts_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Formatter.RenderTables(Array.Empty<IRelationalDatabaseTable>(), new Dictionary<Identifier, ulong>(), null));
        }
    }
}
