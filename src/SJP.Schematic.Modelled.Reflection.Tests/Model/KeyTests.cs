using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model.Tests
{
    [TestFixture]
    public class KeyTests
    {
        [Test]
        public void Ctor_GivenNullColumns_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FakeKey(null, DatabaseKeyType.Primary));
        }

        [Test]
        public void Ctor_GivenEmptyColumns_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FakeKey(Enumerable.Empty<IModelledColumn>(), DatabaseKeyType.Primary));
        }

        [Test]
        public void Ctor_GivenCollectionWithNullColumn_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FakeKey(new List<IModelledColumn> { null }, DatabaseKeyType.Primary));
        }

        [Test]
        public void Ctor_GivenInvalidKeyType_ThrowsArgumentException()
        {
            const DatabaseKeyType badKeyType = (DatabaseKeyType)55;
            Assert.Throws<ArgumentException>(() => new FakeKey(new List<IModelledColumn> { Mock.Of<IModelledColumn>() }, badKeyType));
        }

        private class FakeKey : Key
        {
            public FakeKey(IEnumerable<IModelledColumn> columns, DatabaseKeyType keyType)
                : base(columns, keyType)
            {
            }
        }
    }
}
