using System;
using NUnit.Framework;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests
{
    [TestFixture]
    public class EFCoreModelBuilderTests
    {
        [Test]
        public void Ctor_GivenNullNameProvider_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EFCoreModelBuilder(null, " ", " "));
        }

        [Test]
        public void Ctor_GivenNullLineIndent_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            Assert.Throws<ArgumentNullException>(() => new EFCoreModelBuilder(nameProvider, null, " "));
        }

        [Test]
        public void Ctor_GivenNullIndentLevel_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            Assert.Throws<ArgumentNullException>(() => new EFCoreModelBuilder(nameProvider, " ", null));
        }

        [Test]
        public void AddTable_GivenNullTable_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            var modelBuilder = new EFCoreModelBuilder(nameProvider, " ", " ");

            Assert.Throws<ArgumentNullException>(() => modelBuilder.AddTable(null));
        }

        [Test]
        public void AddSequence_GivenNullTable_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            var modelBuilder = new EFCoreModelBuilder(nameProvider, " ", " ");

            Assert.Throws<ArgumentNullException>(() => modelBuilder.AddSequence(null));
        }
    }
}
