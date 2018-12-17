using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class EmptyDatabaseViewProviderTests
    {
        [Test]
        public static void GetView_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyDatabaseViewProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetView(null));
        }

        [Test]
        public static async Task GetView_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyDatabaseViewProvider();
            var view = provider.GetView("view_name");
            var viewIsNone = await view.IsNone.ConfigureAwait(false);

            Assert.IsTrue(viewIsNone);
        }

        [Test]
        public static async Task GetAllViews_PropertyGet_HasCountOfZero()
        {
            var provider = new EmptyDatabaseViewProvider();
            var views = await provider.GetAllViews().ConfigureAwait(false);

            Assert.Zero(views.Count);
        }

        [Test]
        public static async Task GetAllViews_WhenEnumerated_ContainsNoValues()
        {
            var provider = new EmptyDatabaseViewProvider();
            var views = await provider.GetAllViews().ConfigureAwait(false);
            var viewsList = views.ToList();

            Assert.Zero(viewsList.Count);
        }
    }
}
