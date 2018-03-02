using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Modelled.Reflection.Tests.Fakes;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    internal class ModelledSchemaAttributeTests
    {
        [Test]
        public void Ctor_GivenNullDialects_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FakeModelledSchemaAttribute(null));
        }

        [Test]
        public void Ctor_GivenEmptyDialects_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FakeModelledSchemaAttribute(Enumerable.Empty<Type>()));
        }

        [Test]
        public void Ctor_GivenDialectsWithEmptyValue_ThrowsArgumentNullException()
        {
            var dialects = new List<Type> { null };
            Assert.Throws<ArgumentNullException>(() => new FakeModelledSchemaAttribute(dialects));
        }

        [Test]
        public void Ctor_GivenNonDialectType_ThrowsArgumentNullException()
        {
            var dialects = new[] { typeof(object) };
            Assert.Throws<ArgumentException>(() => new FakeModelledSchemaAttribute(dialects));
        }

        [Test]
        public void Dialects_WhenValidDialectGivenInCtor_ReturnsDialectsInProperty()
        {
            var expectedDialect = typeof(FakeDialect);
            var dialects = new[] { expectedDialect };
            var attr = new FakeModelledSchemaAttribute(dialects);
            var attrDialect = attr.Dialects.Single();

            Assert.AreEqual(expectedDialect, attrDialect);
        }

        [Test]
        public void Dialects_WhenAllDialectsGivenInCtor_ReturnsEmptyCollection()
        {
            var dialects = new[] { Dialect.All };
            var attr = new FakeModelledSchemaAttribute(dialects);
            var attrDialects = attr.Dialects.ToList();

            Assert.Zero(attrDialects.Count);
        }

        [Test]
        public void SupportsDialect_GivenNullDialectType_ThrowsArgumentNullException()
        {
            var dialects = new[] { typeof(FakeDialect) };
            var attr = new FakeModelledSchemaAttribute(dialects);

            Assert.Throws<ArgumentNullException>(() => attr.SupportsDialect(null));
        }

        [Test]
        public void SupportsDialect_GivenNonDialectType_ThrowsArgumentException()
        {
            var dialects = new[] { typeof(FakeDialect) };
            var attr = new FakeModelledSchemaAttribute(dialects);

            Assert.Throws<ArgumentException>(() => attr.SupportsDialect(typeof(object)));
        }

        [Test]
        public void SupportsDialect_GivenMatchingDialectType_ReturnsTrue()
        {
            var dialects = new[] { typeof(FakeDialect) };
            var attr = new FakeModelledSchemaAttribute(dialects);

            var supportsDialect = attr.SupportsDialect(typeof(FakeDialect));

            Assert.IsTrue(supportsDialect);
        }

        [Test]
        public void SupportsDialect_WhenSupportsAllDialects_ReturnsTrue()
        {
            var dialects = new[] { Dialect.All };
            var attr = new FakeModelledSchemaAttribute(dialects);

            var supportsDialect = attr.SupportsDialect(typeof(FakeDialect));

            Assert.IsTrue(supportsDialect);
        }

        [Test]
        public void SupportsDialect_GivenNonMatchingDialectType_ReturnsFalse()
        {
            var dialectMock = Mock.Of<IDatabaseDialect>();
            var mockInstanceType = dialectMock.GetType();
            var dialects = new[] { mockInstanceType };
            var attr = new FakeModelledSchemaAttribute(dialects);

            var supportsDialect = attr.SupportsDialect(typeof(FakeDialect));

            Assert.IsFalse(supportsDialect);
        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
        private class FakeModelledSchemaAttribute : ModelledSchemaAttribute
        {
            public FakeModelledSchemaAttribute(IEnumerable<Type> dialects) : base(dialects)
            {
            }
        }
    }
}
