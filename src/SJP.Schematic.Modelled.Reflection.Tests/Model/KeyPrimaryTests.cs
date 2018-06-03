using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Tests
{
    [TestFixture]
    internal class KeyPrimaryTests
    {
        [Test]
        public void Ctor_GivenNullColumns_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Key.Primary((IModelledColumn[])null));
        }

        [Test]
        public void Ctor_GivenEmptyColumns_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Key.Primary(Array.Empty<IModelledColumn>()));
        }

        [Test]
        public void Ctor_GivenCollectionWithNullColumn_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Key.Primary(new List<IModelledColumn> { null }));
        }
    }
}
