using NUnit.Framework;

namespace SJP.Schematic.Lint.Tests
{
    [TestFixture]
    internal static class RuleMessageTests
    {
        [Test]
        public static void Ctor_GivenNullTitle_ThrowsArgumentNullException()
        {
            const RuleLevel level = RuleLevel.Error;
            const string message = "message";
            Assert.That(() => new RuleMessage(null, level, message), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyTitle_ThrowsArgumentNullException()
        {
            const RuleLevel level = RuleLevel.Error;
            const string message = "message";
            Assert.That(() => new RuleMessage(string.Empty, level, message), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceTitle_ThrowsArgumentNullException()
        {
            const RuleLevel level = RuleLevel.Error;
            const string message = "message";
            Assert.That(() => new RuleMessage("   ", level, message), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenInvalidRuleLevel_ThrowsArgumentException()
        {
            const string title = "title";
            const RuleLevel level = (RuleLevel)999;
            const string message = "message";
            Assert.That(() => new RuleMessage(title, level, message), Throws.ArgumentException);
        }

        [Test]
        public static void Ctor_GivenNullMessage_ThrowsArgumentNullException()
        {
            const string title = "title";
            const RuleLevel level = RuleLevel.Error;
            Assert.That(() => new RuleMessage(title, level, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyMessage_ThrowsArgumentNullException()
        {
            const string title = "title";
            const RuleLevel level = RuleLevel.Error;
            Assert.That(() => new RuleMessage(title, level, string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceMessage_ThrowsArgumentNullException()
        {
            const string title = "title";
            const RuleLevel level = RuleLevel.Error;
            Assert.That(() => new RuleMessage(title, level, "   "), Throws.ArgumentNullException);
        }

        [Test]
        public static void Title_PropertyGet_MatchesCtorArg()
        {
            const string title = "title";
            const RuleLevel level = RuleLevel.Error;
            const string message = "message";

            var ruleMessage = new RuleMessage(title, level, message);

            Assert.That(ruleMessage.Title, Is.EqualTo(title));
        }

        [Test]
        public static void Level_PropertyGet_MatchesCtorArg()
        {
            const string title = "title";
            const RuleLevel level = RuleLevel.Error;
            const string message = "message";

            var ruleMessage = new RuleMessage(title, level, message);

            Assert.That(ruleMessage.Level, Is.EqualTo(level));
        }

        [Test]
        public static void Message_PropertyGet_MatchesCtorArg()
        {
            const string title = "title";
            const RuleLevel level = RuleLevel.Error;
            const string message = "message";

            var ruleMessage = new RuleMessage(title, level, message);

            Assert.That(ruleMessage.Message, Is.EqualTo(message));
        }
    }
}
