using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.OrmLite.Tests
{
    [TestFixture]
    public class DataAccessGeneratorTests
    {
        [Test]
        public void Ctor_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            Assert.Throws<ArgumentNullException>(() => new DataAccessGenerator(null, nameProvider));
        }

        [Test]
        public void Ctor_GivenNullNameProvider_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentNullException>(() => new DataAccessGenerator(database, null));
        }
    }
}
