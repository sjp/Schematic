using NUnit.Framework;
using System;

namespace SJP.Schematic.Modelled.Tests
{
    // TODO, need to create some databases to build with
    [TestFixture]
    public class RelationalDatabaseBuilderTests
    {
        [Test]
        public void Ctor_GivenNullArguments_ThrowsArgumentNullException()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseBuilder((IDependentRelationalDatabase)null));
                Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseBuilder((Func<IDependentRelationalDatabase>)null));
            });
        }
    }
}
