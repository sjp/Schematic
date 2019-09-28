using System;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests
{
    [TestFixture]
    internal static class EFCoreDbContextBuilderTests
    {
        [Test]
        public static void Ctor_GivenNullNameTranslator_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EFCoreDbContextBuilder(null, "test"));
        }

        [Test]
        public static void Ctor_GivenNullNamespace_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new EFCoreDbContextBuilder(nameTranslator, null));
        }

        [Test]
        public static void Ctor_GivenNullIndent_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new EFCoreDbContextBuilder(nameTranslator, "test", null));
        }

        [Test]
        public static void Ctor_GivenEmptyNamespace_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new EFCoreDbContextBuilder(nameTranslator, string.Empty));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceNamespace_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new EFCoreDbContextBuilder(nameTranslator, "   "));
        }

        [Test]
        public static void Generate_GivenNullTables_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");
            var views = Array.Empty<IDatabaseView>();
            var sequences = Array.Empty<IDatabaseSequence>();

            Assert.Throws<ArgumentNullException>(() => dbContextBuilder.Generate(null, views, sequences));
        }

        [Test]
        public static void Generate_GivenNullViews_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");
            var tables = Array.Empty<IRelationalDatabaseTable>();
            var sequences = Array.Empty<IDatabaseSequence>();

            Assert.Throws<ArgumentNullException>(() => dbContextBuilder.Generate(tables, null, sequences));
        }

        [Test]
        public static void Generate_GivenNullSequences_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");
            var tables = Array.Empty<IRelationalDatabaseTable>();
            var views = Array.Empty<IDatabaseView>();

            Assert.Throws<ArgumentNullException>(() => dbContextBuilder.Generate(tables, views, null));
        }
    }
}
