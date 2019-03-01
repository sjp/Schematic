using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Tests.Comments
{
    [TestFixture]
    internal static class EmptyDatabaseViewCommentProviderTests
    {
        [Test]
        public static void GetViewComments_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyDatabaseViewCommentProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetViewComments(null));
        }

        [Test]
        public static async Task GetViewComments_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyDatabaseViewCommentProvider();
            var comment = provider.GetViewComments("test_view");
            var isNone = await comment.IsNone.ConfigureAwait(false);

            Assert.IsTrue(isNone);
        }

        [Test]
        public static async Task GetAllViewComments_WhenInvoked_HasZeroCount()
        {
            var provider = new EmptyDatabaseViewCommentProvider();
            var comments = await provider.GetAllViewComments().ConfigureAwait(false);

            Assert.Zero(comments.Count);
        }

        [Test]
        public static async Task GetAllViewComments_WhenInvoked_DoesNotEnumerateAnyValues()
        {
            var provider = new EmptyDatabaseViewCommentProvider();
            var comments = await provider.GetAllViewComments().ConfigureAwait(false);
            var count = comments.ToList().Count;

            Assert.Zero(count);
        }
    }
}
