using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Tests.Comments
{
    [TestFixture]
    internal static class EmptyRelationalDatabaseCommentProviderTests
    {
        [Test]
        public static void GetTableComments_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyRelationalDatabaseCommentProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetTableComments(null));
        }

        [Test]
        public static async Task GetTableComments_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyRelationalDatabaseCommentProvider();
            var comment = provider.GetTableComments("test_table");
            var isNone = await comment.IsNone.ConfigureAwait(false);

            Assert.IsTrue(isNone);
        }

        [Test]
        public static async Task GetAllTableComments_WhenInvoked_DoesNotEnumerateAnyValues()
        {
            var provider = new EmptyRelationalDatabaseCommentProvider();
            var hasComments = await provider.GetAllTableComments().AnyAsync().ConfigureAwait(false);

            Assert.IsFalse(hasComments);
        }

        [Test]
        public static void GetViewComments_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyRelationalDatabaseCommentProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetViewComments(null));
        }

        [Test]
        public static async Task GetViewComments_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyRelationalDatabaseCommentProvider();
            var comment = provider.GetViewComments("test_view");
            var isNone = await comment.IsNone.ConfigureAwait(false);

            Assert.IsTrue(isNone);
        }

        [Test]
        public static async Task GetAllViewComments_WhenInvoked_DoesNotEnumerateAnyValues()
        {
            var provider = new EmptyRelationalDatabaseCommentProvider();
            var hasComments = await provider.GetAllViewComments()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.IsFalse(hasComments);
        }

        [Test]
        public static void GetSynonymComments_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyRelationalDatabaseCommentProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetSynonymComments(null));
        }

        [Test]
        public static async Task GetSynonymComments_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyRelationalDatabaseCommentProvider();
            var comment = provider.GetSynonymComments("test_synonym");
            var isNone = await comment.IsNone.ConfigureAwait(false);

            Assert.IsTrue(isNone);
        }

        [Test]
        public static async Task GetAllSynonymComments_WhenInvoked_DoesNotEnumerateAnyValues()
        {
            var provider = new EmptyRelationalDatabaseCommentProvider();
            var comments = await provider.GetAllSynonymComments().ToListAsync().ConfigureAwait(false);

            Assert.Zero(comments.Count);
        }

        [Test]
        public static void GetSequenceComments_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyRelationalDatabaseCommentProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetSequenceComments(null));
        }

        [Test]
        public static async Task GetSequenceComments_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyRelationalDatabaseCommentProvider();
            var comment = provider.GetSequenceComments("test_sequence");
            var isNone = await comment.IsNone.ConfigureAwait(false);

            Assert.IsTrue(isNone);
        }

        [Test]
        public static async Task GetAllSequenceComments_WhenInvoked_DoesNotEnumerateAnyValues()
        {
            var provider = new EmptyRelationalDatabaseCommentProvider();
            var hasComments = await provider.GetAllSequenceComments()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.IsFalse(hasComments);
        }

        [Test]
        public static void GetRoutineComments_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyRelationalDatabaseCommentProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetRoutineComments(null));
        }

        [Test]
        public static async Task GetRoutineComments_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyRelationalDatabaseCommentProvider();
            var comment = provider.GetRoutineComments("test_routine");
            var isNone = await comment.IsNone.ConfigureAwait(false);

            Assert.IsTrue(isNone);
        }

        [Test]
        public static async Task GetAllRoutineComments_WhenInvoked_DoesNotEnumerateAnyValues()
        {
            var provider = new EmptyRelationalDatabaseCommentProvider();
            var hasComments = await provider.GetAllRoutineComments()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.IsFalse(hasComments);
        }
    }
}
