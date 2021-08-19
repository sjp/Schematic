using AutoMapper;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Snapshot;

namespace SJP.Schematic.Reporting.Tests.Snapshot
{
    internal static class SnapshotRelationalDatabaseCommentsWriterTests
    {
        [Test]
        public static void Ctor_WhenGivenNullConnectionFactory_ThrowsArgNullException()
        {
            Assert.That(() => new SnapshotRelationalDatabaseCommentsWriter(null, Mock.Of<IMapper>()), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_WhenGivenNullMapper_ThrowsArgNullException()
        {
            Assert.That(() => new SnapshotRelationalDatabaseCommentsWriter(Mock.Of<IDbConnectionFactory>(), null), Throws.ArgumentNullException);
        }

        [Test]
        public static void SnapshotDatabaseCommentsAsync_WhenGivenNullDatabase_ThrowsArgNullException()
        {
            var writer = new SnapshotRelationalDatabaseCommentsWriter(Mock.Of<IDbConnectionFactory>(), Mock.Of<IMapper>());

            Assert.That(() => writer.SnapshotDatabaseCommentsAsync(null), Throws.ArgumentNullException);
        }
    }
}
