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
            Assert.That(() => new FakeKey(null, DatabaseKeyType.Primary), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyColumns_ThrowsArgumentNullException()
        {
            Assert.That(() => new FakeKey(Array.Empty<IModelledColumn>(), DatabaseKeyType.Primary), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenCollectionWithNullColumn_ThrowsArgumentNullException()
        {
            Assert.That(() => new FakeKey(new List<IModelledColumn> { null }, DatabaseKeyType.Primary), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenInvalidKeyType_ThrowsArgumentException()
        {
            const DatabaseKeyType badKeyType = (DatabaseKeyType)55;
            Assert.That(() => new FakeKey(new List<IModelledColumn> { Mock.Of<IModelledColumn>() }, badKeyType), Throws.ArgumentException);
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
