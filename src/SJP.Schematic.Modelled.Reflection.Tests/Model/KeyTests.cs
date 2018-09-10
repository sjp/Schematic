using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model.Tests
{
    [TestFixture]
    internal static class KeyTests
    {
        [Test]
        public static void Ctor_GivenNullColumns_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FakeKey(null, DatabaseKeyType.Primary));
        }

        [Test]
        public static void Ctor_GivenEmptyColumns_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FakeKey(Array.Empty<IModelledColumn>(), DatabaseKeyType.Primary));
        }

        [Test]
        public static void Ctor_GivenCollectionWithNullColumn_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FakeKey(new List<IModelledColumn> { null }, DatabaseKeyType.Primary));
        }

        [Test]
        public static void Ctor_GivenInvalidKeyType_ThrowsArgumentException()
        {
            const DatabaseKeyType badKeyType = (DatabaseKeyType)55;
            Assert.Throws<ArgumentException>(() => new FakeKey(new List<IModelledColumn> { Mock.Of<IModelledColumn>() }, badKeyType));
        }

        private sealed class FakeKey : Key
        {
            public FakeKey(IEnumerable<IModelledColumn> columns, DatabaseKeyType keyType)
                : base(columns, keyType)
            {
            }
        }
    }
}
