using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Tests
{
    [TestFixture]
    internal static class KeyPrimaryTests
    {
        [Test]
        public static void Ctor_GivenNullColumns_ThrowsArgumentNullException()
        {
            Assert.That(() => new Key.Primary((IModelledColumn[])null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyColumns_ThrowsArgumentNullException()
        {
            Assert.That(() => new Key.Primary(Array.Empty<IModelledColumn>()), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenCollectionWithNullColumn_ThrowsArgumentNullException()
        {
            Assert.That(() => new Key.Primary(new List<IModelledColumn> { null }), Throws.ArgumentNullException);
        }
    }
}
