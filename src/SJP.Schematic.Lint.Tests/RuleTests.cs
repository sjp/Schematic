using NUnit.Framework;

namespace SJP.Schematic.Lint.Tests
{
    [TestFixture]
    internal static class RuleTests
    {
        [Test]
        public static void Ctor_GivenNullTitle_ThrowsArgumentNullException()
        {
            const RuleLevel level = RuleLevel.Error;
            Assert.That(() => new FakeRule(null, level), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyTitle_ThrowsArgumentNullException()
        {
            const RuleLevel level = RuleLevel.Error;
            Assert.That(() => new FakeRule(string.Empty, level), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceTitle_ThrowsArgumentNullException()
        {
            const RuleLevel level = RuleLevel.Error;
            Assert.That(() => new FakeRule("   ", level), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenInvalidRuleLevel_ThrowsArgumentException()
        {
            const string title = "test";
            const RuleLevel level = (RuleLevel)999;
            Assert.That(() => new FakeRule(title, level), Throws.ArgumentException);
        }

        [Test]
        public static void Title_PropertyGet_MatchesCtorArg()
        {
            const string title = "test";
            const RuleLevel level = RuleLevel.Error;
            var rule = new FakeRule(title, level);

            Assert.That(rule.Title, Is.EqualTo(title));
        }

        [Test]
        public static void Level_PropertyGet_MatchesCtorArg()
        {
            const string title = "test";
            const RuleLevel level = RuleLevel.Error;
            var rule = new FakeRule(title, level);

            Assert.That(rule.Level, Is.EqualTo(level));
        }

        private sealed class FakeRule : Rule
        {
            public FakeRule(string title, RuleLevel level)
                : base(title, level)
            {
            }
        }
    }
}
