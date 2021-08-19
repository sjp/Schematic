using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Snapshot;

namespace SJP.Schematic.Reporting.Tests.Snapshot
{
    internal static class SnapshotSchemaTests
    {
        [Test]
        public static void Ctor_WhenGivenNullConnectionFactory_ThrowsArgNullException()
        {
            Assert.That(() => new SnapshotSchema(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_WhenGivenValidConnectionFactory_DoesNotThrow()
        {
            Assert.That(() => new SnapshotSchema(Mock.Of<IDbConnectionFactory>()), Throws.Nothing);
        }
    }
}
