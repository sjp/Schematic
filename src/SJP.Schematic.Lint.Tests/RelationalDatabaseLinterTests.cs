using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Tests;

[TestFixture]
internal static class RelationalDatabaseLinterTests
{
    private static IRelationalDatabaseLinter Linter { get; } = new RelationalDatabaseLinter([]);

    private static EmptyRelationalDatabase EmptyDatabase { get; } = new EmptyRelationalDatabase(IdentifierDefaults.Empty);

    [Test]
    public static void Ctor_GivenNullRules_ThrowsArgumentNullException()
    {
        Assert.That(() => new RelationalDatabaseLinter(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
    {
        Assert.That(() => Linter.AnalyseDatabase(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseDatabase_GivenEmptyDatabase_ReturnsEmptyMessages()
    {
        var messages = await Linter.AnalyseDatabase(EmptyDatabase);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        Assert.That(() => Linter.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenEmptyTables_ReturnsEmptyMessages()
    {
        var messages = await Linter.AnalyseTables([]);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static void AnalyseViews_GivenNullViews_ThrowsArgumentNullException()
    {
        Assert.That(() => Linter.AnalyseViews(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseViews_GivenEmptyViews_ReturnsEmptyMessages()
    {
        var messages = await Linter.AnalyseViews([]);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static void AnalyseSequences_GivenNullSequences_ThrowsArgumentNullException()
    {
        Assert.That(() => Linter.AnalyseSequences(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseSequences_GivenEmptySequences_ReturnsEmptyMessages()
    {
        var messages = await Linter.AnalyseSequences([]);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static void AnalyseSynonyms_GivenNullSynonyms_ThrowsArgumentNullException()
    {
        Assert.That(() => Linter.AnalyseSynonyms(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseSynonyms_GivenEmptySynonyms_ReturnsEmptyMessages()
    {
        var messages = await Linter.AnalyseSynonyms([]);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static void AnalyseRoutines_GivenNullRoutines_ThrowsArgumentNullException()
    {
        Assert.That(() => Linter.AnalyseRoutines(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseRoutines_GivenEmptyRoutines_ReturnsEmptyMessages()
    {
        var messages = await Linter.AnalyseRoutines([]);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task Ctor_GivenRulesModifiedAfterConstruction_UsesRulesGivenAtConstruction()
    {
        var rules = new List<IRule>();
        var linter = new RelationalDatabaseLinter(rules);
        rules.Add(new FakeTableRule());

        var messages = await linter.AnalyseTables([]);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task Ctor_GivenLazilyEvaluatedRules_EnumeratesRulesOnce()
    {
        var rules = new EnumerationCountingRules([new FakeTableRule()]);
        var linter = new RelationalDatabaseLinter(rules);

        await linter.AnalyseDatabase(EmptyDatabase);
        await linter.AnalyseTables([]);

        Assert.That(rules.EnumerationCount, Is.EqualTo(1));
    }

    private sealed class EnumerationCountingRules(IEnumerable<IRule> rules) : IEnumerable<IRule>
    {
        public int EnumerationCount { get; private set; }

        public IEnumerator<IRule> GetEnumerator()
        {
            EnumerationCount++;
            return rules.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class FakeTableRule() : Rule("TEST_TABLE_RULE", "test table rule", RuleLevel.Warning), ITableRule
    {
        public Task<IReadOnlyCollection<IRuleMessage>> AnalyseTables(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<IRuleMessage> messages = [new RuleMessage(Id, Title, Level, "a test message")];
            return Task.FromResult(messages);
        }
    }
}