using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing;

internal sealed class ColumnDefinition
{
    public ColumnDefinition(SqlIdentifier columnName)
        : this(columnName.Value.LocalName, [], [])
    {
    }

    public ColumnDefinition(SqlIdentifier columnName, IEnumerable<Token<SqliteToken>> typeDefinition)
        : this(columnName.Value.LocalName, typeDefinition, [])
    {
    }

    public ColumnDefinition(string columnName, IEnumerable<Token<SqliteToken>> typeDefinition, IEnumerable<ColumnConstraint> columnConstraints)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        Name = columnName;
        TypeDefinition = typeDefinition?.ToList() ?? Enumerable.Empty<Token<SqliteToken>>();

        var nullable = true;
        var autoIncrement = false;
        var collation = SqliteCollation.None;
        var defaultValue = new List<Token<SqliteToken>>();
        PrimaryKey? primaryKey = null;
        UniqueKey? uniqueKey = null;
        var foreignKeys = new List<ForeignKey>();
        var checks = new List<Check>();
        var generatedDefinition = new List<Token<SqliteToken>>();
        var generatedColumnType = SqliteGeneratedColumnType.None;

        columnConstraints = columnConstraints?.ToList() ?? Enumerable.Empty<ColumnConstraint>();
        foreach (var constraint in columnConstraints)
        {
            switch (constraint.ConstraintType)
            {
                case ColumnConstraint.ColumnConstraintType.Check:
                    if (constraint is ColumnConstraint.Check ck)
                        checks.Add(new Check(ck.Name, ck.Definition));
                    break;
                case ColumnConstraint.ColumnConstraintType.Collation:
                    if (constraint is ColumnConstraint.Collation col)
                        collation = col.CollationType;
                    break;
                case ColumnConstraint.ColumnConstraintType.Default:
                    if (constraint is ColumnConstraint.DefaultConstraint def)
                        defaultValue.AddRange(def.DefaultValue);
                    break;
                case ColumnConstraint.ColumnConstraintType.ForeignKey:
                    if (constraint is ColumnConstraint.ForeignKey fk)
                        foreignKeys.Add(new ForeignKey(fk.Name, Name, fk.ParentTable, fk.ParentColumnNames));
                    break;
                case ColumnConstraint.ColumnConstraintType.Nullable:
                    if (constraint is ColumnConstraint.Nullable nullableCons)
                        nullable = nullableCons.IsNullable;
                    break;
                case ColumnConstraint.ColumnConstraintType.PrimaryKey:
                    if (constraint is ColumnConstraint.PrimaryKey pk)
                    {
                        autoIncrement = pk.AutoIncrement;
                        primaryKey = new PrimaryKey(pk.Name, new IndexedColumn(Name).WithColumnOrder(pk.ColumnOrder).ToEnumerable());
                    }
                    break;
                case ColumnConstraint.ColumnConstraintType.UniqueKey:
                    if (constraint is ColumnConstraint.UniqueKey uk)
                        uniqueKey = new UniqueKey(uk.Name, Name);
                    break;
                case ColumnConstraint.ColumnConstraintType.GeneratedAlways:
                    if (constraint is ColumnConstraint.GeneratedAlways generated)
                    {
                        generatedDefinition.AddRange(generated.Definition);
                        generatedColumnType = generated.GeneratedColumnType;
                    }
                    break;
            }
        }

        if (primaryKey != null && uniqueKey != null)
            uniqueKey = null; // prefer primary key to unique key

        Nullable = nullable;
        IsAutoIncrement = autoIncrement;
        Collation = collation;
        DefaultValue = defaultValue;
        PrimaryKey = primaryKey != null ? Option<PrimaryKey>.Some(primaryKey) : Option<PrimaryKey>.None;
        UniqueKey = uniqueKey != null ? Option<UniqueKey>.Some(uniqueKey) : Option<UniqueKey>.None;
        ForeignKeys = foreignKeys;
        Checks = checks;
        GeneratedColumnDefinition = generatedDefinition;
        GeneratedColumnType = generatedColumnType;
    }

    public string Name { get; }

    public IEnumerable<Token<SqliteToken>> TypeDefinition { get; }

    public IEnumerable<Token<SqliteToken>> DefaultValue { get; }

    public bool Nullable { get; }

    public bool IsAutoIncrement { get; }

    public SqliteCollation Collation { get; }

    public Option<PrimaryKey> PrimaryKey { get; }

    public Option<UniqueKey> UniqueKey { get; }

    public IEnumerable<ForeignKey> ForeignKeys { get; }

    public IEnumerable<Check> Checks { get; }

    public IEnumerable<Token<SqliteToken>> GeneratedColumnDefinition { get; }

    public SqliteGeneratedColumnType GeneratedColumnType { get; }
}