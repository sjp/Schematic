using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Tests.Comments
{
    [TestFixture]
    internal static class EmptyRelationalDatabaseTableCommentProviderTests
    {
        [Test]
        public static void GetTableComments_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyRelationalDatabaseTableCommentProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetTableComments(null));
        }

        [Test]
        public static async Task GetTableComments_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyRelationalDatabaseTableCommentProvider();
            var comment = provider.GetTableComments("test_table");
            var isNone = await comment.IsNone.ConfigureAwait(false);

            Assert.IsTrue(isNone);
        }

        [Test]
        public static async Task GetAllTableComments_WhenInvoked_HasZeroCount()
        {
            var provider = new EmptyRelationalDatabaseTableCommentProvider();
            var comments = await provider.GetAllTableComments().ConfigureAwait(false);

            Assert.Zero(comments.Count);
        }

        [Test]
        public static async Task GetAllTableComments_WhenInvoked_DoesNotEnumerateAnyValues()
        {
            var provider = new EmptyRelationalDatabaseTableCommentProvider();
            var comments = await provider.GetAllTableComments().ConfigureAwait(false);
            var count = comments.ToList().Count;

            Assert.Zero(count);
        }
    }
}
