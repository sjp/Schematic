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
            Assert.That(() => new Key.Unique((IModelledColumn[])null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyColumns_ThrowsArgumentNullException()
        {
            Assert.That(() => new Key.Unique(Array.Empty<IModelledColumn>()), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenCollectionWithNullColumn_ThrowsArgumentNullException()
        {
            Assert.That(() => new Key.Unique(new List<IModelledColumn> { null }), Throws.ArgumentNullException);
        }
    }
}
