using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Tests
{
    [TestFixture]
    public class KeyPrimaryTests
    {
        [Test]
        public void Ctor_GivenNullColumns_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Key.Primary(null));
        }

        [Test]
        public void Ctor_GivenEmptyColumns_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Key.Primary(Enumerable.Empty<IModelledColumn>()));
        }

        [Test]
        public void Ctor_GivenCollectionWithNullColumn_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Key.Primary(new List<IModelledColumn> { null }));
        }
    }
}
