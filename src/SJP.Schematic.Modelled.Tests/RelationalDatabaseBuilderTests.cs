using System;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Tests
{
    // TODO, need to create some databases to build with
    [TestFixture]
    internal static class RelationalDatabaseBuilderTests
    {
        [Test]
        public static void Ctor_GivenNullArguments_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseBuilder((IRelationalDatabase)null));
                Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseBuilder((Func<IRelationalDatabase>)null));
            });
        }
    }
}
