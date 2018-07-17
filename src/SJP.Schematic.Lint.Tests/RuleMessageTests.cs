using System;
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
            Assert.Throws<ArgumentNullException>(() => new RuleMessage(null, level, message));
        }

        [Test]
        public static void Ctor_GivenEmptyTitle_ThrowsArgumentNullException()
        {
            const RuleLevel level = RuleLevel.Error;
            const string message = "message";
            Assert.Throws<ArgumentNullException>(() => new RuleMessage(string.Empty, level, message));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceTitle_ThrowsArgumentNullException()
        {
            const RuleLevel level = RuleLevel.Error;
            const string message = "message";
            Assert.Throws<ArgumentNullException>(() => new RuleMessage("   ", level, message));
        }

        [Test]
        public static void Ctor_GivenInvalidRuleLevel_ThrowsArgumentException()
        {
            const string title = "title";
            const RuleLevel level = (RuleLevel)999;
            const string message = "message";
            Assert.Throws<ArgumentException>(() => new RuleMessage(title, level, message));
        }

        [Test]
        public static void Ctor_GivenNullMessage_ThrowsArgumentNullException()
        {
            const string title = "title";
            const RuleLevel level = RuleLevel.Error;
            Assert.Throws<ArgumentNullException>(() => new RuleMessage(title, level, null));
        }

        [Test]
        public static void Ctor_GivenEmptyMessage_ThrowsArgumentNullException()
        {
            const string title = "title";
            const RuleLevel level = RuleLevel.Error;
            Assert.Throws<ArgumentNullException>(() => new RuleMessage(title, level, string.Empty));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceMessage_ThrowsArgumentNullException()
        {
            const string title = "title";
            const RuleLevel level = RuleLevel.Error;
            Assert.Throws<ArgumentNullException>(() => new RuleMessage(title, level, "   "));
        }

        [Test]
        public static void Title_PropertyGet_MatchesCtorArg()
        {
            const string title = "title";
            const RuleLevel level = RuleLevel.Error;
            const string message = "message";

            var ruleMessage = new RuleMessage(title, level, message);

            Assert.AreEqual(title, ruleMessage.Title);
        }

        [Test]
        public static void Level_PropertyGet_MatchesCtorArg()
        {
            const string title = "title";
            const RuleLevel level = RuleLevel.Error;
            const string message = "message";

            var ruleMessage = new RuleMessage(title, level, message);

            Assert.AreEqual(level, ruleMessage.Level);
        }

        [Test]
        public static void Message_PropertyGet_MatchesCtorArg()
        {
            const string title = "title";
            const RuleLevel level = RuleLevel.Error;
            const string message = "message";

            var ruleMessage = new RuleMessage(title, level, message);

            Assert.AreEqual(message, ruleMessage.Message);
        }
    }
}
