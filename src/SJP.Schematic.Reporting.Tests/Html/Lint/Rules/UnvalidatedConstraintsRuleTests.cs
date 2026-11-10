using System;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

internal static class UnvalidatedConstraintsRuleTests
{
    private static readonly Identifier TableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

    private static readonly IDatabaseColumn TestColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, Option<DatabaseDefaultValue>.None, null);

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(
            () => new UnvalidatedConstraintsRule(level),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("level")
        );
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new UnvalidatedConstraintsRule(RuleLevel.Error);
        Assert.That(
            () => rule.AnalyseTables(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tables")
        );
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithUnvalidatedPrimaryKey_ProducesMessageWithVisibleTableName()
    {
        var rule = new UnvalidatedConstraintsRule(RuleLevel.Error);

        var table = new RelationalDatabaseTable(
            TableName,
            [],
            Option<DatabaseKey.Primary>.Some(CreatePrimaryKey(false)),
            [],
            [],
            [],
            [],
            [],
            []
        );

        var messages = await rule.AnalyseTables([table]);

        AssertVisibleTableNameMessage(messages.Single());
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithUnvalidatedUniqueKey_ProducesMessageWithVisibleTableName()
    {
        var rule = new UnvalidatedConstraintsRule(RuleLevel.Error);

        var table = new RelationalDatabaseTable(
            TableName,
            [],
            Option<DatabaseKey.Primary>.None,
            [CreateUniqueKey(false)],
            [],
            [],
            [],
            [],
            []
        );

        var messages = await rule.AnalyseTables([table]);

        AssertVisibleTableNameMessage(messages.Single());
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithUnvalidatedForeignKey_ProducesMessageWithVisibleTableName()
    {
        var rule = new UnvalidatedConstraintsRule(RuleLevel.Error);

        var relationalKey = new DatabaseRelationalKey(
            "child_table",
            CreateForeignKey(false),
            "parent_table",
            CreatePrimaryKey(true),
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );

        var table = new RelationalDatabaseTable(
            TableName,
            [],
            Option<DatabaseKey.Primary>.None,
            [],
            [relationalKey],
            [],
            [],
            [],
            []
        );

        var messages = await rule.AnalyseTables([table]);

        AssertVisibleTableNameMessage(messages.Single());
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithUnvalidatedCheckConstraint_ProducesMessageWithVisibleTableName()
    {
        var rule = new UnvalidatedConstraintsRule(RuleLevel.Error);

        var check = new DatabaseCheckConstraint(
            Option<Identifier>.Some("test_check"),
            "test_column is not null",
            true,
            false,
            ConstraintDeferrability.NotDeferrable
        );

        var table = new RelationalDatabaseTable(
            TableName,
            [],
            Option<DatabaseKey.Primary>.None,
            [],
            [],
            [],
            [],
            [check],
            []
        );

        var messages = await rule.AnalyseTables([table]);

        AssertVisibleTableNameMessage(messages.Single());
    }

    private static DatabaseKey.Primary CreatePrimaryKey(bool isValidated)
    {
        return new DatabaseKey.Primary(
            Option<Identifier>.Some("test_key"),
            [TestColumn],
            true,
            Option<IDatabaseIndex>.None,
            isValidated,
            ConstraintDeferrability.NotDeferrable
        );
    }

    private static DatabaseKey.Unique CreateUniqueKey(bool isValidated)
    {
        return new DatabaseKey.Unique(
            Option<Identifier>.Some("test_key"),
            [TestColumn],
            true,
            Option<IDatabaseIndex>.None,
            isValidated,
            ConstraintDeferrability.NotDeferrable
        );
    }

    private static DatabaseKey.Foreign CreateForeignKey(bool isValidated)
    {
        return new DatabaseKey.Foreign(
            Option<Identifier>.Some("test_key"),
            [TestColumn],
            true,
            isValidated,
            ConstraintDeferrability.NotDeferrable
        );
    }

    private static void AssertVisibleTableNameMessage(IRuleMessage message)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Message, Does.Contain("test_schema.test_table"));
            Assert.That(message.Message, Does.Not.Contain("LocalName ="));
        }
    }
}
