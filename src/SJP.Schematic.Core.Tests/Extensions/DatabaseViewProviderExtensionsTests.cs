using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Tests.Fakes;

namespace SJP.Schematic.Core.Tests.Extensions
{
    [TestFixture]
    internal static class DatabaseViewProviderExtensionsTests
    {
        private static FakeRelationalDatabase GetFakeDatabase()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            return new FakeRelationalDatabase(dialect, connection, identifierDefaults);
        }

        private static IDatabaseView GetMockView(Identifier viewName)
        {
            var viewMock = new Mock<IDatabaseView>();
            viewMock.SetupGet(v => v.Name).Returns(viewName);

            return viewMock.Object;
        }

        [Test]
        public static void TryGetViewAsync_GivenNullDatabase_ThrowsArgumentNullException()
        {
            IRelationalDatabase database = null;
            var viewName = new Identifier("A");

            Assert.Throws<ArgumentNullException>(() => database.TryGetViewAsync(viewName));
        }

        [Test]
        public static void TryGetViewAsync_GivenNullViewName_ThrowsArgumentNullException()
        {
            var database = GetFakeDatabase();
            Identifier viewName = null;

            Assert.Throws<ArgumentNullException>(() => database.TryGetViewAsync(viewName));
        }

        [Test]
        public static async Task TryGetViewAsync_GivenPresentViewName_ReturnsTrue()
        {
            var fakeDb = GetFakeDatabase();
            var viewName = new Identifier("A");
            var view = GetMockView(viewName);
            fakeDb.Views = new[] { view };

            var result = await fakeDb.TryGetViewAsync(viewName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsTrue(result.exists);
        }

        [Test]
        public static async Task TryGetViewAsync_GivenPresentViewName_ReturnsCorrectView()
        {
            var fakeDb = GetFakeDatabase();
            var viewName = new Identifier("A");
            var view = GetMockView(viewName);
            fakeDb.Views = new[] { view };

            var result = await fakeDb.TryGetViewAsync(viewName, CancellationToken.None).ConfigureAwait(false);
            Assert.AreEqual(view, result.view);
        }

        [Test]
        public static async Task TryGetViewAsync_GivenMissingViewName_ReturnsFalse()
        {
            var fakeDb = GetFakeDatabase();
            var viewName = new Identifier("A");
            var (exists, _) = await fakeDb.TryGetViewAsync(viewName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsFalse(exists);
        }

        [Test]
        public static async Task TryGetViewAsync_GivenMissingViewName_ReturnsNullView()
        {
            var fakeDb = GetFakeDatabase();
            var viewName = new Identifier("A");
            var (_, view) = await fakeDb.TryGetViewAsync(viewName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsNull(view);
        }
    }
}
