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
            Assert.Throws<ArgumentNullException>(() => new Key.Primary((IModelledColumn[])null));
        }

        [Test]
        public static void Ctor_GivenEmptyColumns_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Key.Primary(Array.Empty<IModelledColumn>()));
        }

        [Test]
        public static void Ctor_GivenCollectionWithNullColumn_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Key.Primary(new List<IModelledColumn> { null }));
        }
    }
}
