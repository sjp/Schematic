using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests
{
    [TestFixture]
    internal static class EFCoreDbContextBuilderTests
    {
        [Test]
        public static void Ctor_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            Assert.Throws<ArgumentNullException>(() => new EFCoreDbContextBuilder(null, nameProvider, "testns"));
        }

        [Test]
        public static void Ctor_GivenNullNameProvider_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentNullException>(() => new EFCoreDbContextBuilder(database, null, "testns"));
        }

        [Test]
        public static void Ctor_GivenNullNamespace_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var nameProvider = new VerbatimNameProvider();
            Assert.Throws<ArgumentNullException>(() => new EFCoreDbContextBuilder(database, nameProvider, null));
        }

        [Test]
        public static void Ctor_GivenEmptyNamespace_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var nameProvider = new VerbatimNameProvider();
            Assert.Throws<ArgumentNullException>(() => new EFCoreDbContextBuilder(database, nameProvider, string.Empty));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceNamespace_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var nameProvider = new VerbatimNameProvider();
            Assert.Throws<ArgumentNullException>(() => new EFCoreDbContextBuilder(database, nameProvider, "   "));
        }
    }
}
