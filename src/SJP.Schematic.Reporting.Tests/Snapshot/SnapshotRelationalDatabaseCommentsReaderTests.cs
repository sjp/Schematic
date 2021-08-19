using AutoMapper;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Snapshot;

namespace SJP.Schematic.Reporting.Tests.Snapshot
{
    internal static class SnapshotRelationalDatabaseCommentsReaderTests
    {
        [Test]
        public static void Ctor_WhenGivenNullConnectionFactory_ThrowsArgNullException()
        {
            Assert.That(() => new SnapshotRelationalDatabaseCommentsReader(null, Mock.Of<IMapper>()), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_WhenGivenNullMapper_ThrowsArgNullException()
        {
            Assert.That(() => new SnapshotRelationalDatabaseCommentsReader(Mock.Of<IDbConnectionFactory>(), null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetTableComments_WhenGivenNullTableName_ThrowsArgNullException()
        {
            var reader = new SnapshotRelationalDatabaseCommentsReader(Mock.Of<IDbConnectionFactory>(), Mock.Of<IMapper>());

            Assert.That(() => reader.GetTableComments(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetViewComments_WhenGivenNullViewName_ThrowsArgNullException()
        {
            var reader = new SnapshotRelationalDatabaseCommentsReader(Mock.Of<IDbConnectionFactory>(), Mock.Of<IMapper>());

            Assert.That(() => reader.GetViewComments(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSequenceComments_WhenGivenNullSequenceName_ThrowsArgNullException()
        {
            var reader = new SnapshotRelationalDatabaseCommentsReader(Mock.Of<IDbConnectionFactory>(), Mock.Of<IMapper>());

            Assert.That(() => reader.GetSequenceComments(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSynonymComments_WhenGivenNullSynonymName_ThrowsArgNullException()
        {
            var reader = new SnapshotRelationalDatabaseCommentsReader(Mock.Of<IDbConnectionFactory>(), Mock.Of<IMapper>());

            Assert.That(() => reader.GetSynonymComments(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetRoutineComments_WhenGivenNullRoutineName_ThrowsArgNullException()
        {
            var reader = new SnapshotRelationalDatabaseCommentsReader(Mock.Of<IDbConnectionFactory>(), Mock.Of<IMapper>());

            Assert.That(() => reader.GetRoutineComments(null), Throws.ArgumentNullException);
        }
    }
}
