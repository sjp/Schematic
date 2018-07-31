using System;
using System.Collections.Generic;
using Superpower;
using Superpower.Model;
using System.Linq;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Parsing
{
    public class SqliteTableParser
    {
        public SqliteTableParser(TokenList<SqliteToken> tokens, string definition)
        {
            if (tokens == default(TokenList<SqliteToken>) || tokens.Empty())
                throw new ArgumentNullException(nameof(tokens));

            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            var parsedTable = ParseTokens(tokens);
            Columns = parsedTable.Columns;
            PrimaryKey = parsedTable.PrimaryKey;
            UniqueKeys = parsedTable.UniqueKeys;
            ParentKeys = parsedTable.ParentKeys;
            Checks = parsedTable.Checks;
        }

        public string Definition { get; }

        public IEnumerable<Column> Columns { get; }

        public PrimaryKey PrimaryKey { get; }

        public IEnumerable<UniqueKey> UniqueKeys { get; }

        public IEnumerable<ForeignKey> ParentKeys { get; }

        public IEnumerable<Check> Checks { get; }

        private static ParsedTable ParseTokens(TokenList<SqliteToken> tokens)
        {
            var next = tokens.ConsumeToken();
            var createTablePrefix = SqliteTokenParsers.CreateTablePrefix(next.Location);
            if (!createTablePrefix.HasValue)
                throw new ArgumentException("Token list does not start with a CREATE TABLE statement.", nameof(tokens));

            next = createTablePrefix.Remainder.ConsumeToken();
            var ifNotExists = SqliteTokenParsers.IfNotExistsClause(next.Location);
            if (ifNotExists.HasValue)
                next = ifNotExists.Remainder.ConsumeToken();

            var tableName = SqliteTokenParsers.QualifiedName(next.Location);
            if (!tableName.HasValue)
                throw new ArgumentException("Unable to determine the name of the table being parsed.", nameof(tokens));

            next = tableName.Remainder.ConsumeToken();
            var isSelectBasedTable = !next.HasValue || next.Value.Kind != SqliteToken.LParen;

            // skipping because we cannot parse extra information from a select-based table
            if (isSelectBasedTable)
                return ParsedTable.Empty;

            next = next.Remainder.ConsumeToken(); // consume LParen

            var tableMembers = SqliteTokenParsers.TableMembers(next.Location);
            if (!tableMembers.HasValue)
            {
                var errMessage = "Unable to parse columns and/or constraints from the table definition. Error: " + tableMembers.ErrorMessage;
                throw new ArgumentException(errMessage, nameof(tokens));
            }

            var columns = new List<Column>();
            PrimaryKey primaryKey = null;
            var uniqueKeys = new List<UniqueKey>();
            var foreignKeys = new List<ForeignKey>();
            var checks = new List<Check>();

            var parsedColumns = tableMembers.Value.SelectMany(m => m.Columns);
            var parsedConstraints = tableMembers.Value.SelectMany(m => m.Constraints);

            foreach (var parsedColumn in parsedColumns)
            {
                var column = new Column(
                    parsedColumn.Name,
                    parsedColumn.TypeDefinition,
                    parsedColumn.Nullable,
                    parsedColumn.IsAutoIncrement,
                    parsedColumn.Collation,
                    parsedColumn.DefaultValue
                );

                columns.Add(column);

                primaryKey = primaryKey ?? parsedColumn.PrimaryKey;
                if (parsedColumn.UniqueKey != null)
                    uniqueKeys.Add(parsedColumn.UniqueKey);
                foreignKeys.AddRange(parsedColumn.ForeignKeys);
                checks.AddRange(parsedColumn.Checks);
            }

            foreach (var parsedConstraint in parsedConstraints)
            {
                switch (parsedConstraint.ConstraintType)
                {
                    case TableConstraint.TableConstraintType.PrimaryKey:
                        var pk = parsedConstraint as TableConstraint.PrimaryKey;
                        primaryKey = new PrimaryKey(pk.Name, pk.Columns);
                        break;
                    case TableConstraint.TableConstraintType.UniqueKey:
                        var uk = parsedConstraint as TableConstraint.UniqueKey;
                        uniqueKeys.Add(new UniqueKey(uk.Name, uk.Columns));
                        break;
                    case TableConstraint.TableConstraintType.ForeignKey:
                        var fk = parsedConstraint as TableConstraint.ForeignKey;
                        foreignKeys.Add(new ForeignKey(fk.Name, fk.Columns, fk.ParentTable, fk.ParentColumnNames));
                        break;
                    case TableConstraint.TableConstraintType.Check:
                        var ck = parsedConstraint as TableConstraint.Check;
                        checks.Add(new Check(ck.Name, ck.Definition));
                        break;
                }
            }

            return new ParsedTable(columns, primaryKey, uniqueKeys, foreignKeys, checks);
        }

        // only used for structured return from ParseTokens()
        private class ParsedTable
        {
            public ParsedTable(
                IReadOnlyCollection<Column> columns,
                PrimaryKey primaryKey,
                IReadOnlyCollection<UniqueKey> uniqueKeys,
                IReadOnlyCollection<ForeignKey> parentKeys,
                IReadOnlyCollection<Check> checks
            )
            {
                if (columns == null || columns.Empty())
                    throw new ArgumentNullException(nameof(columns));
                if (uniqueKeys == null)
                    throw new ArgumentNullException(nameof(uniqueKeys));
                if (checks == null)
                    throw new ArgumentNullException(nameof(checks));
                if (parentKeys == null)
                    throw new ArgumentNullException(nameof(parentKeys));

                Columns = columns;
                PrimaryKey = primaryKey;
                UniqueKeys = uniqueKeys;
                Checks = checks;
                ParentKeys = parentKeys;
            }

            private ParsedTable()
            {
                Columns = Array.Empty<Column>();
                UniqueKeys = Array.Empty<UniqueKey>();
                Checks = Array.Empty<Check>();
                ParentKeys = Array.Empty<ForeignKey>();
            }

            public IEnumerable<Column> Columns { get; }

            public PrimaryKey PrimaryKey { get; }

            public IEnumerable<UniqueKey> UniqueKeys { get; }

            public IEnumerable<Check> Checks { get; }

            public IEnumerable<ForeignKey> ParentKeys { get; }

            public static ParsedTable Empty => new ParsedTable();
        }
    }
}
