using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class EmptyRelationalDatabaseViewProviderTests
    {
        [Test]
        public static void GetViewAsync_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyRelationalDatabaseViewProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetViewAsync(null));
        }

        [Test]
        public static async Task GetViewAsync_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyRelationalDatabaseViewProvider();
            var view = provider.GetViewAsync("view_name");
            var viewIsNone = await view.IsNone.ConfigureAwait(false);

            Assert.IsTrue(viewIsNone);
        }

        [Test]
        public static async Task ViewsAsync_PropertyGet_HasCountOfZero()
        {
            var provider = new EmptyRelationalDatabaseViewProvider();
            var views = await provider.ViewsAsync().ConfigureAwait(false);

            Assert.Zero(views.Count);
        }

        [Test]
        public static async Task ViewsAsync_WhenEnumerated_ContainsNoValues()
        {
            var provider = new EmptyRelationalDatabaseViewProvider();
            var views = await provider.ViewsAsync().ConfigureAwait(false);
            var viewsList = views.ToList();

            Assert.Zero(viewsList.Count);
        }
    }
}
