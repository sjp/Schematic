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
            Assert.That(() => provider.GetTableComments(null), Throws.ArgumentNullException);
        }

        [Test]
        public static async Task GetTableComments_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyRelationalDatabaseTableCommentProvider();
            var comment = provider.GetTableComments("test_table");
            var isNone = await comment.IsNone.ConfigureAwait(false);

            Assert.That(isNone, Is.True);
        }

        [Test]
        public static async Task GetAllTableComments_WhenInvoked_DoesNotEnumerateAnyValues()
        {
            var provider = new EmptyRelationalDatabaseTableCommentProvider();
            var hasComments = await provider.GetAllTableComments().AnyAsync().ConfigureAwait(false);

            Assert.That(hasComments, Is.False);
        }
    }
}
