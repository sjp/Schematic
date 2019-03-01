using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Tests.Comments
{
    [TestFixture]
    internal static class EmptyDatabaseSynonymCommentProviderTests
    {
        [Test]
        public static void GetSynonymComments_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyDatabaseSynonymCommentProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetSynonymComments(null));
        }

        [Test]
        public static async Task GetSynonymComments_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyDatabaseSynonymCommentProvider();
            var comment = provider.GetSynonymComments("test_synonym");
            var isNone = await comment.IsNone.ConfigureAwait(false);

            Assert.IsTrue(isNone);
        }

        [Test]
        public static async Task GetAllSynonymComments_WhenInvoked_HasZeroCount()
        {
            var provider = new EmptyDatabaseSynonymCommentProvider();
            var comments = await provider.GetAllSynonymComments().ConfigureAwait(false);

            Assert.Zero(comments.Count);
        }

        [Test]
        public static async Task GetAllSynonymComments_WhenInvoked_DoesNotEnumerateAnyValues()
        {
            var provider = new EmptyDatabaseSynonymCommentProvider();
            var comments = await provider.GetAllSynonymComments().ConfigureAwait(false);
            var count = comments.ToList().Count;

            Assert.Zero(count);
        }
    }
}
