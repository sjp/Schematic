using NUnit.Framework;

namespace SJP.Schematic.Lint.Tests
{
    [TestFixture]
    internal static class RuleTests
    {
        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void Ctor_GivenNullOrWhiteSpaceId_ThrowsArgumentNullException(string id)
        {
            const RuleLevel level = RuleLevel.Error;
            Assert.That(() => new FakeRule(id, "test_title", level), Throws.ArgumentNullException);
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void Ctor_GivenNullOrWhiteSpaceTitle_ThrowsArgumentNullException(string title)
        {
            const RuleLevel level = RuleLevel.Error;
            Assert.That(() => new FakeRule("TEST_ID", title, level), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenInvalidRuleLevel_ThrowsArgumentException()
        {
            const string id = "TEST_ID";
            const string title = "test";
            const RuleLevel level = (RuleLevel)999;
            Assert.That(() => new FakeRule(id, title, level), Throws.ArgumentException);
        }

        [Test]
        public static void Id_PropertyGet_MatchesCtorArg()
        {
            const string id = "TEST_ID";
            const string title = "test";
            const RuleLevel level = RuleLevel.Error;
            var rule = new FakeRule(id, title, level);

            Assert.That(rule.Id, Is.EqualTo(id));
        }

        [Test]
        public static void Title_PropertyGet_MatchesCtorArg()
        {
            const string id = "TEST_ID";
            const string title = "test";
            const RuleLevel level = RuleLevel.Error;
            var rule = new FakeRule(id, title, level);

            Assert.That(rule.Title, Is.EqualTo(title));
        }

        [Test]
        public static void Level_PropertyGet_MatchesCtorArg()
        {
            const string id = "TEST_ID";
            const string title = "test";
            const RuleLevel level = RuleLevel.Error;
            var rule = new FakeRule(id, title, level);

            Assert.That(rule.Level, Is.EqualTo(level));
        }

        private sealed class FakeRule : Rule
        {
            public FakeRule(string id, string title, RuleLevel level)
                : base(id, title, level)
            {
            }
        }
    }
}
