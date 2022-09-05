using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing;

internal abstract class ColumnConstraint
{
    public enum ColumnConstraintType
    {
        Default,
        Collation,
        PrimaryKey,
        UniqueKey,
        ForeignKey,
        Check,
        Nullable,
        GeneratedAlways
    }

    protected ColumnConstraint(ColumnConstraintType constraintType)
    {
        if (!constraintType.IsValid())
            throw new ArgumentException($"The {nameof(ColumnConstraintType)} provided must be a valid enum.", nameof(constraintType));

        ConstraintType = constraintType;
    }

    public Option<string> Name { get; protected set; }

    public ColumnConstraintType ConstraintType { get; }

    public ColumnConstraint WithName(SqlIdentifier identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        Name = Option<string>.Some(identifier.Value.LocalName);
        return this;
    }

    public sealed class Collation : ColumnConstraint
    {
        public Collation(Token<SqliteToken> collation)
            : base(ColumnConstraintType.Collation)
        {
            var collationName = collation.ToStringValue();

            CollationType = Enum.TryParse(collationName, out SqliteCollation type)
                ? type
                : SqliteCollation.None;
        }

        public SqliteCollation CollationType { get; }
    }

    public sealed class Nullable : ColumnConstraint
    {
        public Nullable(bool isNullable)
            : base(ColumnConstraintType.Nullable)
        {
            IsNullable = isNullable;
        }

        public bool IsNullable { get; }
    }

    public sealed class DefaultConstraint : ColumnConstraint
    {
        public DefaultConstraint(IReadOnlyCollection<Token<SqliteToken>> tokens)
            : base(ColumnConstraintType.Default)
        {
            if (tokens.NullOrEmpty())
                throw new ArgumentNullException(nameof(tokens));

            DefaultValue = tokens;
        }

        public IReadOnlyCollection<Token<SqliteToken>> DefaultValue { get; }
    }

    public sealed class PrimaryKey : ColumnConstraint
    {
        public PrimaryKey(IndexColumnOrder columnOrder = IndexColumnOrder.Ascending, bool autoIncrement = false)
            : base(ColumnConstraintType.PrimaryKey)
        {
            if (!columnOrder.IsValid())
                throw new ArgumentException($"The {nameof(IndexColumnOrder)} provided must be a valid enum.", nameof(columnOrder));

            ColumnOrder = columnOrder;
            AutoIncrement = autoIncrement;
        }

        public IndexColumnOrder ColumnOrder { get; }

        public bool AutoIncrement { get; }
    }

    public sealed class UniqueKey : ColumnConstraint
    {
        public UniqueKey()
            : base(ColumnConstraintType.UniqueKey)
        {
        }
    }

    public sealed class ForeignKey : ColumnConstraint
    {
        public ForeignKey(SqlIdentifier parentTableName, IReadOnlyCollection<SqlIdentifier> parentColumnNames)
            : base(ColumnConstraintType.ForeignKey)
        {
            ArgumentNullException.ThrowIfNull(parentTableName);
            if (parentColumnNames.NullOrEmpty())
                throw new ArgumentNullException(nameof(parentColumnNames));

            ParentTable = parentTableName.Value;
            ParentColumnNames = parentColumnNames.Select(static c => c.Value.LocalName).ToList();
        }

        public Identifier ParentTable { get; }

        public IReadOnlyCollection<string> ParentColumnNames { get; }
    }

    public sealed class Check : ColumnConstraint
    {
        public Check(SqlExpression definition)
            : base(ColumnConstraintType.Check)
        {
            if (definition?.Tokens.NullOrEmpty() == true)
                throw new ArgumentNullException(nameof(definition));

            Definition = definition!.Tokens.ToList();
        }

        public IReadOnlyCollection<Token<SqliteToken>> Definition { get; }
    }

    public sealed class GeneratedAlways : ColumnConstraint
    {
        public GeneratedAlways(SqlExpression definition)
            : this(definition, SqliteGeneratedColumnType.Virtual)
        {
        }

        public GeneratedAlways(SqlExpression definition, SqliteGeneratedColumnType generatedColumnType)
            : base(ColumnConstraintType.GeneratedAlways)
        {
            if (definition?.Tokens.NullOrEmpty() == true)
                throw new ArgumentNullException(nameof(definition));

            Definition = definition!.Tokens.ToList();
            GeneratedColumnType = generatedColumnType;
        }

        public IReadOnlyCollection<Token<SqliteToken>> Definition { get; }

        public SqliteGeneratedColumnType GeneratedColumnType { get; }
    }
}