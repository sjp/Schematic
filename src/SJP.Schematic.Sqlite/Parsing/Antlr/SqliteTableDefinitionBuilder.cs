using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;
using static SJP.Schematic.Sqlite.Parsing.Antlr.AntlrParsingExtensions;

namespace SJP.Schematic.Sqlite.Parsing.Antlr;

/// <summary>
/// Translates an ANTLR <c>CREATE TABLE</c> parse tree into <see cref="ParsedTableData"/>.
/// </summary>
internal static class SqliteTableDefinitionBuilder
{
    public static ParsedTableData Build(string definition, SQLiteParser.Create_table_stmtContext context)
    {
        // SELECT-based tables (CREATE TABLE ... AS SELECT) carry no parseable column/constraint
        // information, mirroring the behaviour of the previous parser.
        if (context.column_def().Length == 0)
            return ParsedTableData.Empty(definition);

        var columns = new List<Column>();
        var primaryKey = Option<PrimaryKey>.None;
        var uniqueKeys = new List<UniqueKey>();
        var foreignKeys = new List<ForeignKey>();
        var checks = new List<Check>();

        foreach (var columnDef in context.column_def())
        {
            var columnName = UnquoteIdentifier(columnDef.column_name().GetText());
            var typeName = columnDef.type_name();
            var typeDefinition = typeName != null ? typeName.OriginalText() : string.Empty;

            var nullable = true;
            var autoIncrement = false;
            var collation = SqliteCollation.None;
            var defaultValue = string.Empty;
            var computedDefinition = string.Empty;
            var computedColumnType = SqliteGeneratedColumnType.None;

            foreach (var columnConstraint in columnDef.column_constraint())
            {
                var constraintName = GetConstraintName(columnConstraint.name());

                if (columnConstraint.PRIMARY_() != null && columnConstraint.KEY_() != null)
                {
                    if (primaryKey.IsNone)
                        primaryKey = Option<PrimaryKey>.Some(new PrimaryKey(constraintName, columnName));
                    if (columnConstraint.AUTOINCREMENT_() != null)
                        autoIncrement = true;
                }
                else if (columnConstraint.NULL_() != null)
                {
                    nullable = columnConstraint.NOT_() == null;
                }
                else if (columnConstraint.UNIQUE_() != null)
                {
                    uniqueKeys.Add(new UniqueKey(constraintName, columnName));
                }
                else if (columnConstraint.COLLATE_() != null)
                {
                    collation = MapCollation(columnConstraint.collation_name().GetText());
                }
                else if (columnConstraint.DEFAULT_() != null)
                {
                    defaultValue = GetDefaultValueText(columnConstraint);
                }
                else if (columnConstraint.foreign_key_clause() != null)
                {
                    BuildForeignKey(constraintName, [columnName], columnConstraint.foreign_key_clause())
                        .IfSome(foreignKeys.Add);
                }
                else if (columnConstraint.AS_() != null)
                {
                    computedDefinition = OriginalText(columnConstraint.OPEN_PAR(), columnConstraint.CLOSE_PAR());
                    computedColumnType = columnConstraint.STORED_() != null
                        ? SqliteGeneratedColumnType.Stored
                        : SqliteGeneratedColumnType.Virtual;
                }
                else if (columnConstraint.CHECK_() != null)
                {
                    checks.Add(new Check(constraintName, OriginalText(columnConstraint.OPEN_PAR(), columnConstraint.CLOSE_PAR())));
                }
            }

            columns.Add(new Column(
                columnName,
                typeDefinition,
                nullable,
                autoIncrement,
                collation,
                defaultValue,
                computedDefinition,
                computedColumnType
            ));
        }

        foreach (var tableConstraint in context.table_constraint())
        {
            var constraintName = GetConstraintName(tableConstraint.name());

            if (tableConstraint.PRIMARY_() != null && tableConstraint.KEY_() != null)
            {
                var cols = tableConstraint.indexed_column().Select(BuildIndexedColumn).ToList();
                primaryKey = Option<PrimaryKey>.Some(new PrimaryKey(constraintName, cols));
            }
            else if (tableConstraint.UNIQUE_() != null)
            {
                var cols = tableConstraint.indexed_column().Select(BuildIndexedColumn).ToList();
                uniqueKeys.Add(new UniqueKey(constraintName, cols));
            }
            else if (tableConstraint.FOREIGN_() != null)
            {
                var childColumns = tableConstraint.column_name()
                    .Select(c => UnquoteIdentifier(c.GetText()))
                    .ToList();
                BuildForeignKey(constraintName, childColumns, tableConstraint.foreign_key_clause())
                    .IfSome(foreignKeys.Add);
            }
            else if (tableConstraint.CHECK_() != null)
            {
                checks.Add(new Check(constraintName, OriginalText(tableConstraint.OPEN_PAR(), tableConstraint.CLOSE_PAR())));
            }
        }

        return new ParsedTableData(
            definition,
            columns,
            primaryKey,
            uniqueKeys,
            foreignKeys,
            checks
        );
    }

    private static Option<string> GetConstraintName(SQLiteParser.NameContext? name)
        => name != null
            ? Option<string>.Some(UnquoteIdentifier(name.GetText()))
            : Option<string>.None;

    private static string GetDefaultValueText(SQLiteParser.Column_constraintContext columnConstraint)
    {
        if (columnConstraint.signed_number() != null)
            return columnConstraint.signed_number().OriginalText();
        if (columnConstraint.literal_value() != null)
            return columnConstraint.literal_value().OriginalText();
        if (columnConstraint.OPEN_PAR() != null && columnConstraint.CLOSE_PAR() != null)
            return OriginalText(columnConstraint.OPEN_PAR(), columnConstraint.CLOSE_PAR());

        return string.Empty;
    }

    private static IndexedColumn BuildIndexedColumn(SQLiteParser.Indexed_columnContext indexedColumn)
    {
        var name = TryGetSimpleColumnName(indexedColumn.expr());
        var expression = name == null ? indexedColumn.expr().OriginalText() : string.Empty;
        var collation = indexedColumn.COLLATE_() != null
            ? MapCollation(indexedColumn.collation_name().GetText())
            : SqliteCollation.None;
        var columnOrder = indexedColumn.asc_desc()?.DESC_() != null
            ? IndexColumnOrder.Descending
            : IndexColumnOrder.Ascending;

        return new IndexedColumn(name, expression, collation, columnOrder);
    }

    private static Option<ForeignKey> BuildForeignKey(Option<string> constraintName, IReadOnlyCollection<string> childColumns, SQLiteParser.Foreign_key_clauseContext clause)
    {
        var parentTable = Identifier.CreateQualifiedIdentifier(UnquoteIdentifier(clause.foreign_table().GetText()));
        var parentColumns = clause.column_name()
            .Select(c => UnquoteIdentifier(c.GetText()))
            .ToList();

        // The referenced columns may be omitted (implying the parent's primary key), in which
        // case the relationship cannot be expressed by column name and is left for the database
        // metadata to resolve.
        if (parentColumns.Count == 0 || parentColumns.Count != childColumns.Count)
            return Option<ForeignKey>.None;

        return Option<ForeignKey>.Some(new ForeignKey(constraintName, childColumns, parentTable, parentColumns));
    }
}
