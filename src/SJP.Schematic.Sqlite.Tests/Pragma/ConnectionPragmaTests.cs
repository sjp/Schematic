using System.Data;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests.Pragma
{
    [TestFixture]
    internal static class ConnectionPragmaTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.That(() => new ConnectionPragma(null), Throws.ArgumentNullException);
        }
    }
}
