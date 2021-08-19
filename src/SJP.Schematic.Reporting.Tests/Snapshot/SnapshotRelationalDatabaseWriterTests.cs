using AutoMapper;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Snapshot;

namespace SJP.Schematic.Reporting.Tests.Snapshot
{
    internal static class SnapshotRelationalDatabaseWriterTests
    {
        [Test]
        public static void Ctor_WhenGivenNullConnectionFactory_ThrowsArgNullException()
        {
            Assert.That(() => new SnapshotRelationalDatabaseWriter(null, Mock.Of<IMapper>()), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_WhenGivenNullMapper_ThrowsArgNullException()
        {
            Assert.That(() => new SnapshotRelationalDatabaseWriter(Mock.Of<IDbConnectionFactory>(), null), Throws.ArgumentNullException);
        }

        [Test]
        public static void SnapshotDatabaseObjectsAsync_WhenGivenNullDatabase_ThrowsArgNullException()
        {
            var writer = new SnapshotRelationalDatabaseWriter(Mock.Of<IDbConnectionFactory>(), Mock.Of<IMapper>());

            Assert.That(() => writer.SnapshotDatabaseObjectsAsync(null), Throws.ArgumentNullException);
        }
    }
}
