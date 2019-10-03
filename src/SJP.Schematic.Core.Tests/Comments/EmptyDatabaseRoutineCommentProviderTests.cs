using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Tests.Comments
{
    [TestFixture]
    internal static class EmptyDatabaseRoutineCommentProviderTests
    {
        [Test]
        public static void GetRoutineComments_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyDatabaseRoutineCommentProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetRoutineComments(null));
        }

        [Test]
        public static async Task GetRoutineComments_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyDatabaseRoutineCommentProvider();
            var comment = provider.GetRoutineComments("test_routine");
            var isNone = await comment.IsNone.ConfigureAwait(false);

            Assert.IsTrue(isNone);
        }

        [Test]
        public static async Task GetAllRoutineComments_WhenInvoked_DoesNotEnumerateAnyValues()
        {
            var provider = new EmptyDatabaseRoutineCommentProvider();
            var hasComments = await provider.GetAllRoutineComments()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.IsFalse(hasComments);
        }
    }
}
