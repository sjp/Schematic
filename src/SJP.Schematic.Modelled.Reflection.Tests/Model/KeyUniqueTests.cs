using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Tests
{
    [TestFixture]
    internal static class KeyUniqueTests
    {
        [Test]
        public static void Ctor_GivenNullColumns_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Key.Unique((IModelledColumn[])null));
        }

        [Test]
        public static void Ctor_GivenEmptyColumns_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Key.Unique(Array.Empty<IModelledColumn>()));
        }

        [Test]
        public static void Ctor_GivenCollectionWithNullColumn_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Key.Unique(new List<IModelledColumn> { null }));
        }
    }
}
