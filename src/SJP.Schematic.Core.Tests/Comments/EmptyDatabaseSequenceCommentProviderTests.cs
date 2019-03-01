using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Tests.Comments
{
    [TestFixture]
    internal static class EmptyDatabaseSequenceCommentProviderTests
    {
        [Test]
        public static void GetSequenceComments_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyDatabaseSequenceCommentProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetSequenceComments(null));
        }

        [Test]
        public static async Task GetSequenceComments_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyDatabaseSequenceCommentProvider();
            var comment = provider.GetSequenceComments("test_sequence");
            var isNone = await comment.IsNone.ConfigureAwait(false);

            Assert.IsTrue(isNone);
        }

        [Test]
        public static async Task GetAllSequenceComments_WhenInvoked_HasZeroCount()
        {
            var provider = new EmptyDatabaseSequenceCommentProvider();
            var comments = await provider.GetAllSequenceComments().ConfigureAwait(false);

            Assert.Zero(comments.Count);
        }

        [Test]
        public static async Task GetAllSequenceComments_WhenInvoked_DoesNotEnumerateAnyValues()
        {
            var provider = new EmptyDatabaseSequenceCommentProvider();
            var comments = await provider.GetAllSequenceComments().ConfigureAwait(false);
            var count = comments.ToList().Count;

            Assert.Zero(count);
        }
    }
}
