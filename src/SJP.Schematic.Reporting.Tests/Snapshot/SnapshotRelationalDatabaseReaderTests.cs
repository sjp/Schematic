using AutoMapper;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Snapshot;

namespace SJP.Schematic.Reporting.Tests.Snapshot
{
    internal static class SnapshotRelationalDatabaseReaderTests
    {
        [Test]
        public static void Ctor_WhenGivenNullConnectionFactory_ThrowsArgNullException()
        {
            Assert.That(() => new SnapshotRelationalDatabaseReader(null, Mock.Of<IMapper>()), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_WhenGivenNullMapper_ThrowsArgNullException()
        {
            Assert.That(() => new SnapshotRelationalDatabaseReader(Mock.Of<IDbConnectionFactory>(), null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetTable_WhenGivenNullTableName_ThrowsArgNullException()
        {
            var reader = new SnapshotRelationalDatabaseReader(Mock.Of<IDbConnectionFactory>(), Mock.Of<IMapper>());

            Assert.That(() => reader.GetTable(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetView_WhenGivenNullViewName_ThrowsArgNullException()
        {
            var reader = new SnapshotRelationalDatabaseReader(Mock.Of<IDbConnectionFactory>(), Mock.Of<IMapper>());

            Assert.That(() => reader.GetView(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSequence_WhenGivenNullSequenceName_ThrowsArgNullException()
        {
            var reader = new SnapshotRelationalDatabaseReader(Mock.Of<IDbConnectionFactory>(), Mock.Of<IMapper>());

            Assert.That(() => reader.GetSequence(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSynonym_WhenGivenNullSynonymName_ThrowsArgNullException()
        {
            var reader = new SnapshotRelationalDatabaseReader(Mock.Of<IDbConnectionFactory>(), Mock.Of<IMapper>());

            Assert.That(() => reader.GetSynonym(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetRoutine_WhenGivenNullRoutineName_ThrowsArgNullException()
        {
            var reader = new SnapshotRelationalDatabaseReader(Mock.Of<IDbConnectionFactory>(), Mock.Of<IMapper>());

            Assert.That(() => reader.GetRoutine(null), Throws.ArgumentNullException);
        }
    }
}
