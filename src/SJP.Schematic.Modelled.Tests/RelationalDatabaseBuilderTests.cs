using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Modelled.Tests
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
