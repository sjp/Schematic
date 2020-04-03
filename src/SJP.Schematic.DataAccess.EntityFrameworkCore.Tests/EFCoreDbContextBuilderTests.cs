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
            Assert.That(() => new EFCoreDbContextBuilder(null, "test"), Throws.ArgumentNullException);
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void Ctor_GivenNullOrWhiteSpaceNamespace_ThrowsArgumentNullException(string ns)
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.That(() => new EFCoreDbContextBuilder(nameTranslator, ns), Throws.ArgumentNullException);
        }

        [Test]
        public static void Generate_GivenNullTables_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");
            var views = Array.Empty<IDatabaseView>();
            var sequences = Array.Empty<IDatabaseSequence>();

            Assert.That(() => dbContextBuilder.Generate(null, views, sequences), Throws.ArgumentNullException);
        }

        [Test]
        public static void Generate_GivenNullViews_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");
            var tables = Array.Empty<IRelationalDatabaseTable>();
            var sequences = Array.Empty<IDatabaseSequence>();

            Assert.That(() => dbContextBuilder.Generate(tables, null, sequences), Throws.ArgumentNullException);
        }

        [Test]
        public static void Generate_GivenNullSequences_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");
            var tables = Array.Empty<IRelationalDatabaseTable>();
            var views = Array.Empty<IDatabaseView>();

            Assert.That(() => dbContextBuilder.Generate(tables, views, null), Throws.ArgumentNullException);
        }
    }
}
