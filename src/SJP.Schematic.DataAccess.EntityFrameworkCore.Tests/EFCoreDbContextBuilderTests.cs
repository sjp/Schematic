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

        [Test]
        public static void Ctor_GivenNullNamespace_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.That(() => new EFCoreDbContextBuilder(nameTranslator, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyNamespace_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.That(() => new EFCoreDbContextBuilder(nameTranslator, string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceNamespace_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.That(() => new EFCoreDbContextBuilder(nameTranslator, "   "), Throws.ArgumentNullException);
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
