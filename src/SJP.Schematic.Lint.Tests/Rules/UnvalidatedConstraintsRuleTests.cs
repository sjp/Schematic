using System;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

internal static class UnvalidatedConstraintsRuleTests
{
    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new UnvalidatedConstraintsRule(level), Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("level"));
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new UnvalidatedConstraintsRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tables"));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithValidatedConstraints_ProducesNoMessages()
    {
        var rule = new UnvalidatedConstraintsRule(RuleLevel.Error);

        var table = CreateTable(
            primaryKey: CreatePrimaryKey(isEnabled: true, isValidated: true),
            check: CreateCheck(isEnabled: true, isValidated: true)
        );

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithUnvalidatedPrimaryKey_ProducesMessages()
    {
        var rule = new UnvalidatedConstraintsRule(RuleLevel.Error);

        var table = CreateTable(primaryKey: CreatePrimaryKey(isEnabled: true, isValidated: false));

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Has.Count.EqualTo(1));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithUnvalidatedUniqueKey_ProducesMessages()
    {
        var rule = new UnvalidatedConstraintsRule(RuleLevel.Error);

        var table = CreateTable(uniqueKey: CreateUniqueKey(isEnabled: true, isValidated: false));

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Has.Count.EqualTo(1));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithUnvalidatedForeignKey_ProducesMessages()
    {
        var rule = new UnvalidatedConstraintsRule(RuleLevel.Error);

        var table = CreateTable(relationalKey: CreateRelationalKey(isEnabled: true, isValidated: false));

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Has.Count.EqualTo(1));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithUnvalidatedCheckConstraint_ProducesMessages()
    {
        var rule = new UnvalidatedConstraintsRule(RuleLevel.Error);

        var table = CreateTable(check: CreateCheck(isEnabled: true, isValidated: false));

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Has.Count.EqualTo(1));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithDisabledUnvalidatedConstraints_ProducesNoMessages()
    {
        var rule = new UnvalidatedConstraintsRule(RuleLevel.Error);

        var table = CreateTable(
            primaryKey: CreatePrimaryKey(isEnabled: false, isValidated: false),
            uniqueKey: CreateUniqueKey(isEnabled: false, isValidated: false),
            relationalKey: CreateRelationalKey(isEnabled: false, isValidated: false),
            check: CreateCheck(isEnabled: false, isValidated: false)
        );

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Is.Empty);
    }

    private static readonly IDatabaseColumn TestColumn = new DatabaseColumn(
        "test_column",
        Mock.Of<IDbType>(),
        false,
        Option<DatabaseDefaultValue>.None,
        new ColumnValueSource.Stored()
    );

    private static DatabaseKey.Primary CreatePrimaryKey(bool isEnabled, bool isValidated)
    {
        return new DatabaseKey.Primary(
            Option<Identifier>.Some("test_key"),
            [TestColumn],
            isEnabled,
            Option<IDatabaseIndex>.None,
            isValidated,
            ConstraintDeferrability.NotDeferrable
        );
    }

    private static DatabaseKey.Unique CreateUniqueKey(bool isEnabled, bool isValidated)
    {
        return new DatabaseKey.Unique(
            Option<Identifier>.Some("test_key"),
            [TestColumn],
            isEnabled,
            Option<IDatabaseIndex>.None,
            isValidated,
            ConstraintDeferrability.NotDeferrable
        );
    }

    private static DatabaseKey.Foreign CreateForeignKey(bool isEnabled, bool isValidated)
    {
        return new DatabaseKey.Foreign(
            Option<Identifier>.Some("test_key"),
            [TestColumn],
            isEnabled,
            isValidated,
            ConstraintDeferrability.NotDeferrable
        );
    }

    private static DatabaseCheckConstraint CreateCheck(bool isEnabled, bool isValidated)
    {
        return new DatabaseCheckConstraint(
            Option<Identifier>.Some("test_check"),
            "test_column is not null",
            isEnabled,
            isValidated,
            ConstraintDeferrability.NotDeferrable
        );
    }

    private static DatabaseRelationalKey CreateRelationalKey(bool isEnabled, bool isValidated)
    {
        return new DatabaseRelationalKey(
            "child_table",
            CreateForeignKey(isEnabled, isValidated),
            "parent_table",
            CreatePrimaryKey(true, true),
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );
    }

    private static RelationalDatabaseTable CreateTable(
        DatabaseKey.Primary primaryKey = null,
        DatabaseKey.Unique uniqueKey = null,
        IDatabaseRelationalKey relationalKey = null,
        IDatabaseCheckConstraint check = null
    )
    {
        return new RelationalDatabaseTable(
            "test",
            [],
            primaryKey == null ? Option<DatabaseKey.Primary>.None : Option<DatabaseKey.Primary>.Some(primaryKey),
            uniqueKey == null ? [] : [uniqueKey],
            relationalKey == null ? [] : [relationalKey],
            [],
            [],
            check == null ? [] : [check],
            []
        );
    }
}
