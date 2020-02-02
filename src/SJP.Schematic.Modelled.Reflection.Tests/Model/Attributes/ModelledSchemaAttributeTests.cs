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
    internal static class ModelledSchemaAttributeTests
    {
        [Test]
        public static void Ctor_GivenNullDialects_ThrowsArgumentNullException()
        {
            Assert.That(() => new FakeModelledSchemaAttribute(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyDialects_ThrowsArgumentNullException()
        {
            Assert.That(() => new FakeModelledSchemaAttribute(Array.Empty<Type>()), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenDialectsWithEmptyValue_ThrowsArgumentNullException()
        {
            var dialects = new List<Type> { null };
            Assert.That(() => new FakeModelledSchemaAttribute(dialects), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNonDialectType_ThrowsArgumentNullException()
        {
            var dialects = new[] { typeof(object) };
            Assert.That(() => new FakeModelledSchemaAttribute(dialects), Throws.ArgumentException);
        }

        [Test]
        public static void Dialects_WhenValidDialectGivenInCtor_ReturnsDialectsInProperty()
        {
            var expectedDialect = typeof(FakeDialect);
            var dialects = new[] { expectedDialect };
            var attr = new FakeModelledSchemaAttribute(dialects);
            var attrDialect = attr.Dialects.Single();

            Assert.That(attrDialect, Is.EqualTo(expectedDialect));
        }

        [Test]
        public static void Dialects_WhenAllDialectsGivenInCtor_ReturnsEmptyCollection()
        {
            var dialects = new[] { Dialect.All };
            var attr = new FakeModelledSchemaAttribute(dialects);
            var attrDialects = attr.Dialects.ToList();

            Assert.That(attrDialects, Is.Empty);
        }

        [Test]
        public static void SupportsDialect_GivenNullDialectType_ThrowsArgumentNullException()
        {
            var dialects = new[] { typeof(FakeDialect) };
            var attr = new FakeModelledSchemaAttribute(dialects);

            Assert.That(() => attr.SupportsDialect(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void SupportsDialect_GivenNonDialectType_ThrowsArgumentException()
        {
            var dialects = new[] { typeof(FakeDialect) };
            var attr = new FakeModelledSchemaAttribute(dialects);

            Assert.That(() => attr.SupportsDialect(typeof(object)), Throws.ArgumentException);
        }

        [Test]
        public static void SupportsDialect_GivenMatchingDialectType_ReturnsTrue()
        {
            var dialects = new[] { typeof(FakeDialect) };
            var attr = new FakeModelledSchemaAttribute(dialects);

            var supportsDialect = attr.SupportsDialect(typeof(FakeDialect));

            Assert.That(supportsDialect, Is.True);
        }

        [Test]
        public static void SupportsDialect_WhenSupportsAllDialects_ReturnsTrue()
        {
            var dialects = new[] { Dialect.All };
            var attr = new FakeModelledSchemaAttribute(dialects);

            var supportsDialect = attr.SupportsDialect(typeof(FakeDialect));

            Assert.That(supportsDialect, Is.True);
        }

        [Test]
        public static void SupportsDialect_GivenNonMatchingDialectType_ReturnsFalse()
        {
            var dialectMock = Mock.Of<IDatabaseDialect>();
            var mockInstanceType = dialectMock.GetType();
            var dialects = new[] { mockInstanceType };
            var attr = new FakeModelledSchemaAttribute(dialects);

            var supportsDialect = attr.SupportsDialect(typeof(FakeDialect));

            Assert.That(supportsDialect, Is.False);
        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
        private sealed class FakeModelledSchemaAttribute : ModelledSchemaAttribute
        {
            public FakeModelledSchemaAttribute(IEnumerable<Type> dialects) : base(dialects)
            {
            }
        }
    }
}
