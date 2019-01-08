using System;
using NUnit.Framework;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests
{
    [TestFixture]
    internal static class EFCoreModelBuilderTests
    {
        [Test]
        public static void Ctor_GivenNullNameTranslator_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EFCoreModelBuilder(null, " ", " "));
        }

        [Test]
        public static void Ctor_GivenNullLineIndent_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new EFCoreModelBuilder(nameTranslator, null, " "));
        }

        [Test]
        public static void Ctor_GivenNullIndentLevel_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new EFCoreModelBuilder(nameTranslator, " ", null));
        }

        [Test]
        public static void AddTable_GivenNullTable_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            var modelBuilder = new EFCoreModelBuilder(nameTranslator, " ", " ");

            Assert.Throws<ArgumentNullException>(() => modelBuilder.AddTable(null));
        }

        [Test]
        public static void AddSequence_GivenNullTable_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            var modelBuilder = new EFCoreModelBuilder(nameTranslator, " ", " ");

            Assert.Throws<ArgumentNullException>(() => modelBuilder.AddSequence(null));
        }
    }
}
