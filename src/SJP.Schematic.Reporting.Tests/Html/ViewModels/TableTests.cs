using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels;

internal static class TableTests
{
    private static readonly Identifier TableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");
    private static readonly Identifier ParentTableName = Identifier.CreateQualifiedIdentifier("test_schema", "parent_table");

    [Test]
    public static void Ctor_GivenNullTableName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table(null!, [], Option<Table.PrimaryKeyConstraint>.None, [], [], [], [], [], TableKind.Regular, Option<Table.Partitioning>.None, Option<Table.SystemVersioning>.None, true, Option<Identifier>.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName")
        );
    }

    [Test]
    public static void Ctor_GivenInvalidTableKind_ThrowsArgumentException()
    {
        Assert.That(
            () => new Table(TableName, [], Option<Table.PrimaryKeyConstraint>.None, [], [], [], [], [], (TableKind)55, Option<Table.Partitioning>.None, Option<Table.SystemVersioning>.None, true, Option<Identifier>.None),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("kind")
        );
    }

    [Test]
    public static void Ctor_GivenNullColumns_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table(TableName, null!, Option<Table.PrimaryKeyConstraint>.None, [], [], [], [], [], TableKind.Regular, Option<Table.Partitioning>.None, Option<Table.SystemVersioning>.None, true, Option<Identifier>.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columns")
        );
    }

    [Test]
    public static void Ctor_GivenNullUniqueKeys_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table(TableName, [], Option<Table.PrimaryKeyConstraint>.None, null!, [], [], [], [], TableKind.Regular, Option<Table.Partitioning>.None, Option<Table.SystemVersioning>.None, true, Option<Identifier>.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("uniqueKeys")
        );
    }

    [Test]
    public static void Ctor_GivenNullForeignKeys_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table(TableName, [], Option<Table.PrimaryKeyConstraint>.None, [], null!, [], [], [], TableKind.Regular, Option<Table.Partitioning>.None, Option<Table.SystemVersioning>.None, true, Option<Identifier>.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("foreignKeys")
        );
    }

    [Test]
    public static void Ctor_GivenNullChecks_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table(TableName, [], Option<Table.PrimaryKeyConstraint>.None, [], [], null!, [], [], TableKind.Regular, Option<Table.Partitioning>.None, Option<Table.SystemVersioning>.None, true, Option<Identifier>.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("checks")
        );
    }

    [Test]
    public static void Ctor_GivenNullIndexes_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table(TableName, [], Option<Table.PrimaryKeyConstraint>.None, [], [], [], null!, [], TableKind.Regular, Option<Table.Partitioning>.None, Option<Table.SystemVersioning>.None, true, Option<Identifier>.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("indexes")
        );
    }

    [Test]
    public static void Ctor_GivenNullTriggers_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table(TableName, [], Option<Table.PrimaryKeyConstraint>.None, [], [], [], [], null!, TableKind.Regular, Option<Table.Partitioning>.None, Option<Table.SystemVersioning>.None, true, Option<Identifier>.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("triggers")
        );
    }

    [Test]
    public static void PartitioningCtor_GivenNullStrategy_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.Partitioning(null!, [], []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("strategy")
        );
    }

    [Test]
    public static void PartitioningCtor_GivenNullColumnNames_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.Partitioning("RANGE", null!, []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnNames")
        );
    }

    [Test]
    public static void PartitioningCtor_GivenNullPartitions_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.Partitioning("RANGE", [], null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("partitions")
        );
    }

    [Test]
    public static void SystemVersioningCtor_GivenNullHistoryTable_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.SystemVersioning(null!, "valid_from", "valid_to"),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("historyTable")
        );
    }

    [Test]
    public static void SystemVersioningCtor_GivenNullPeriodStartColumn_ThrowsArgumentNullException()
    {
        var historyTable = new Table.LinkedTable(TableName, true);

        Assert.That(
            () => new Table.SystemVersioning(historyTable, null!, "valid_to"),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("periodStartColumn")
        );
    }

    [Test]
    public static void SystemVersioningCtor_GivenNullPeriodEndColumn_ThrowsArgumentNullException()
    {
        var historyTable = new Table.LinkedTable(TableName, true);

        Assert.That(
            () => new Table.SystemVersioning(historyTable, "valid_from", null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("periodEndColumn")
        );
    }

    [Test]
    public static void LinkedTableCtor_GivenNullName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.LinkedTable(null!, true),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("name")
        );
    }

    [Test]
    public static void ColumnCtor_GivenInvalidComputedColumnStorage_ThrowsArgumentException()
    {
        Assert.That(
            () => new Table.Column("test_column", 1, false, "integer", Option<Uri>.None, Option<string>.None, false, false, false, [], [], Option<IAutoIncrement>.None, false, Option<string>.None, (ComputedColumnStorage)55, false),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("computedStorage")
        );
    }

    [Test]
    public static void ColumnCtor_GivenNullColumnName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.Column(null!, 1, false, "integer", Option<Uri>.None, Option<string>.None, false, false, false, [], [], Option<IAutoIncrement>.None, false, Option<string>.None, ComputedColumnStorage.Unknown, false),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnName")
        );
    }

    [Test]
    public static void ColumnCtor_GivenNullChildKeys_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.Column("test_column", 1, false, "integer", Option<Uri>.None, Option<string>.None, false, false, false, null!, [], Option<IAutoIncrement>.None, false, Option<string>.None, ComputedColumnStorage.Unknown, false),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("childKeys")
        );
    }

    [Test]
    public static void ColumnCtor_GivenNullParentKeys_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.Column("test_column", 1, false, "integer", Option<Uri>.None, Option<string>.None, false, false, false, [], null!, Option<IAutoIncrement>.None, false, Option<string>.None, ComputedColumnStorage.Unknown, false),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("parentKeys")
        );
    }

    [Test]
    public static void PrimaryKeyConstraintCtor_GivenInvalidConstraintDeferrability_ThrowsArgumentException()
    {
        Assert.That(
            () => new Table.PrimaryKeyConstraint("test_pk", ["test_column"], true, (ConstraintDeferrability)55),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("deferrability")
        );
    }

    [Test]
    public static void PrimaryKeyConstraintCtor_GivenNullColumns_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.PrimaryKeyConstraint("test_pk", null!, true, ConstraintDeferrability.NotDeferrable),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columns")
        );
    }

    [Test]
    public static void PrimaryKeyConstraintCtor_GivenEmptyColumns_ThrowsArgumentException()
    {
        Assert.That(
            () => new Table.PrimaryKeyConstraint("test_pk", [], true, ConstraintDeferrability.NotDeferrable),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columns")
        );
    }

    [Test]
    public static void UniqueKeyCtor_GivenInvalidConstraintDeferrability_ThrowsArgumentException()
    {
        Assert.That(
            () => new Table.UniqueKey("test_uk", ["test_column"], true, (ConstraintDeferrability)55),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("deferrability")
        );
    }

    [Test]
    public static void UniqueKeyCtor_GivenNullColumns_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.UniqueKey("test_uk", null!, true, ConstraintDeferrability.NotDeferrable),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columns")
        );
    }

    [Test]
    public static void UniqueKeyCtor_GivenEmptyColumns_ThrowsArgumentException()
    {
        Assert.That(
            () => new Table.UniqueKey("test_uk", [], true, ConstraintDeferrability.NotDeferrable),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columns")
        );
    }

    [Test]
    public static void ForeignKeyCtor_GivenInvalidConstraintDeferrability_ThrowsArgumentException()
    {
        Assert.That(
            () => new Table.ForeignKey("test_fk", ["child_column"], ParentTableName, "parent_pk", ["parent_column"], ReferentialAction.NoAction, ReferentialAction.NoAction, true, (ConstraintDeferrability)55, ForeignKeyMatchType.Simple),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("deferrability")
        );
    }

    [Test]
    public static void ForeignKeyCtor_GivenNullColumnNames_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.ForeignKey("test_fk", null!, ParentTableName, "parent_pk", ["parent_column"], ReferentialAction.NoAction, ReferentialAction.NoAction, true, ConstraintDeferrability.NotDeferrable, ForeignKeyMatchType.Simple),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnNames")
        );
    }

    [Test]
    public static void ForeignKeyCtor_GivenEmptyColumnNames_ThrowsArgumentException()
    {
        Assert.That(
            () => new Table.ForeignKey("test_fk", [], ParentTableName, "parent_pk", ["parent_column"], ReferentialAction.NoAction, ReferentialAction.NoAction, true, ConstraintDeferrability.NotDeferrable, ForeignKeyMatchType.Simple),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnNames")
        );
    }

    [Test]
    public static void ForeignKeyCtor_GivenNullParentTableName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.ForeignKey("test_fk", ["child_column"], null!, "parent_pk", ["parent_column"], ReferentialAction.NoAction, ReferentialAction.NoAction, true, ConstraintDeferrability.NotDeferrable, ForeignKeyMatchType.Simple),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("parentTableName")
        );
    }

    [Test]
    public static void ForeignKeyCtor_GivenNullParentColumnNames_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.ForeignKey("test_fk", ["child_column"], ParentTableName, "parent_pk", null!, ReferentialAction.NoAction, ReferentialAction.NoAction, true, ConstraintDeferrability.NotDeferrable, ForeignKeyMatchType.Simple),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("parentColumnNames")
        );
    }

    [Test]
    public static void ForeignKeyCtor_GivenEmptyParentColumnNames_ThrowsArgumentException()
    {
        Assert.That(
            () => new Table.ForeignKey("test_fk", ["child_column"], ParentTableName, "parent_pk", [], ReferentialAction.NoAction, ReferentialAction.NoAction, true, ConstraintDeferrability.NotDeferrable, ForeignKeyMatchType.Simple),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("parentColumnNames")
        );
    }

    [Test]
    public static void ForeignKeyCtor_GivenInvalidDeleteAction_ThrowsArgumentException()
    {
        Assert.That(
            () => new Table.ForeignKey("test_fk", ["child_column"], ParentTableName, "parent_pk", ["parent_column"], (ReferentialAction)55, ReferentialAction.NoAction, true, ConstraintDeferrability.NotDeferrable, ForeignKeyMatchType.Simple),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("deleteAction")
        );
    }

    [Test]
    public static void ForeignKeyCtor_GivenInvalidUpdateAction_ThrowsArgumentException()
    {
        Assert.That(
            () => new Table.ForeignKey("test_fk", ["child_column"], ParentTableName, "parent_pk", ["parent_column"], ReferentialAction.NoAction, (ReferentialAction)55, true, ConstraintDeferrability.NotDeferrable, ForeignKeyMatchType.Simple),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("updateAction")
        );
    }

    [Test]
    public static void ForeignKeyCtor_GivenInvalidMatchType_ThrowsArgumentException()
    {
        Assert.That(
            () => new Table.ForeignKey("test_fk", ["child_column"], ParentTableName, "parent_pk", ["parent_column"], ReferentialAction.NoAction, ReferentialAction.NoAction, true, ConstraintDeferrability.NotDeferrable, (ForeignKeyMatchType)55),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("matchType")
        );
    }

    [Test]
    public static void CheckConstraintCtor_GivenInvalidConstraintDeferrability_ThrowsArgumentException()
    {
        Assert.That(
            () => new Table.CheckConstraint("test_ck", "test_column > 1", true, (ConstraintDeferrability)55),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("deferrability")
        );
    }

    [Test]
    public static void CheckConstraintCtor_GivenNullDefinition_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.CheckConstraint("test_ck", null!, true, ConstraintDeferrability.NotDeferrable),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("definition")
        );
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void CheckConstraintCtor_GivenEmptyOrWhiteSpaceDefinition_ThrowsArgumentException(string definition)
    {
        Assert.That(
            () => new Table.CheckConstraint("test_ck", definition, true, ConstraintDeferrability.NotDeferrable),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("definition")
        );
    }

    [Test]
    public static void IndexCtor_GivenNullColumnSorts_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.Index("test_index", false, ["test_column"], null!, [], IndexType.BTree, Option<string>.None, true, true, true),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnSorts")
        );
    }

    [Test]
    public static void IndexCtor_GivenInvalidIndexColumnOrder_ThrowsArgumentException()
    {
        Assert.That(
            () => new Table.Index("test_index", false, ["test_column"], [(IndexColumnOrder)55], [], IndexType.BTree, Option<string>.None, true, true, true),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnSorts")
        );
    }

    [Test]
    public static void IndexCtor_GivenInvalidIndexType_ThrowsArgumentException()
    {
        Assert.That(
            () => new Table.Index("test_index", false, ["test_column"], [IndexColumnOrder.Ascending], [], (IndexType)55, Option<string>.None, true, true, true),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("indexType")
        );
    }

    [Test]
    public static void TriggerCtor_GivenNullTriggerName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.Trigger(null!, "create trigger test_trigger...", TriggerQueryTiming.Before, TriggerEvent.Insert, TriggerGranularity.Row, Option<string>.None, []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("triggerName")
        );
    }

    [Test]
    public static void TriggerCtor_GivenNullDefinition_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.Trigger("test_trigger", null!, TriggerQueryTiming.Before, TriggerEvent.Insert, TriggerGranularity.Row, Option<string>.None, []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("definition")
        );
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void TriggerCtor_GivenEmptyOrWhiteSpaceDefinition_ThrowsArgumentException(string definition)
    {
        Assert.That(
            () => new Table.Trigger("test_trigger", definition, TriggerQueryTiming.Before, TriggerEvent.Insert, TriggerGranularity.Row, Option<string>.None, []),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("definition")
        );
    }

    [Test]
    public static void TriggerCtor_GivenNullUpdateColumns_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.Trigger("test_trigger", "create trigger test_trigger...", TriggerQueryTiming.Before, TriggerEvent.Insert, TriggerGranularity.Row, Option<string>.None, null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("updateColumns")
        );
    }

    [Test]
    public static void TriggerCtor_GivenInvalidTriggerEvent_ThrowsArgumentException()
    {
        Assert.That(
            () => new Table.Trigger("test_trigger", "create trigger test_trigger...", TriggerQueryTiming.Before, (TriggerEvent)55, TriggerGranularity.Row, Option<string>.None, []),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("triggerEvent")
        );
    }

    [Test]
    public static void TriggerCtor_GivenNoTriggerEvents_ThrowsArgumentException()
    {
        Assert.That(
            () => new Table.Trigger("test_trigger", "create trigger test_trigger...", TriggerQueryTiming.Before, TriggerEvent.None, TriggerGranularity.Row, Option<string>.None, []),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("triggerEvent")
        );
    }

    [Test]
    public static void TriggerCtor_GivenUndefinedTriggerEventFlag_DoesNotSilentlyDropIt()
    {
        Assert.That(
            () => new Table.Trigger("test_trigger", "create trigger test_trigger...", TriggerQueryTiming.Before, TriggerEvent.Insert | (TriggerEvent)64, TriggerGranularity.Row, Option<string>.None, []),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("triggerEvent")
        );
    }

    [Test]
    public static void ParentKeyCtor_GivenNullParentTableName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.ParentKey("test_fk", null!, "parent_column", "test_table.child_column"),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("parentTableName")
        );
    }

    [Test]
    public static void ParentKeyCtor_GivenNullParentColumnName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.ParentKey("test_fk", ParentTableName, null!, "test_table.child_column"),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("parentColumnName")
        );
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void ParentKeyCtor_GivenEmptyOrWhiteSpaceParentColumnName_ThrowsArgumentException(string parentColumnName)
    {
        Assert.That(
            () => new Table.ParentKey("test_fk", ParentTableName, parentColumnName, "test_table.child_column"),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("parentColumnName")
        );
    }

    [Test]
    public static void ParentKeyCtor_GivenNullQualifiedChildColumnName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.ParentKey("test_fk", ParentTableName, "parent_column", null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("qualifiedChildColumnName")
        );
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void ParentKeyCtor_GivenEmptyOrWhiteSpaceQualifiedChildColumnName_ThrowsArgumentException(string qualifiedChildColumnName)
    {
        Assert.That(
            () => new Table.ParentKey("test_fk", ParentTableName, "parent_column", qualifiedChildColumnName),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("qualifiedChildColumnName")
        );
    }

    [Test]
    public static void ChildKeyCtor_GivenNullChildTableName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.ChildKey("test_fk", null!, "child_column", "parent_table.parent_column"),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("childTableName")
        );
    }

    [Test]
    public static void ChildKeyCtor_GivenNullChildColumnName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.ChildKey("test_fk", TableName, null!, "parent_table.parent_column"),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("childColumnName")
        );
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void ChildKeyCtor_GivenEmptyOrWhiteSpaceChildColumnName_ThrowsArgumentException(string childColumnName)
    {
        Assert.That(
            () => new Table.ChildKey("test_fk", TableName, childColumnName, "parent_table.parent_column"),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("childColumnName")
        );
    }

    [Test]
    public static void ChildKeyCtor_GivenNullQualifiedParentColumnName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Table.ChildKey("test_fk", TableName, "child_column", null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("qualifiedParentColumnName")
        );
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void ChildKeyCtor_GivenEmptyOrWhiteSpaceQualifiedParentColumnName_ThrowsArgumentException(string qualifiedParentColumnName)
    {
        Assert.That(
            () => new Table.ChildKey("test_fk", TableName, "child_column", qualifiedParentColumnName),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("qualifiedParentColumnName")
        );
    }
}
