using System;
using System.Collections.Generic;
using System.Linq;
using Superpower.Model;
using SJP.Schematic.Core.Extensions;
using LanguageExt;

namespace SJP.Schematic.Sqlite.Parsing
{
    public class SqliteTableParser
    {
        public ParsedTableData ParseTokens(string definition, TokenList<SqliteToken> tokens)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));
            if (tokens == default || tokens.Empty())
                throw new ArgumentNullException(nameof(tokens));

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
                return ParsedTableData.Empty(definition);

            next = next.Remainder.ConsumeToken(); // consume LParen

            var tableMembers = SqliteTokenParsers.TableMembers(next.Location);
            if (!tableMembers.HasValue)
            {
                var errMessage = "Unable to parse columns and/or constraints from the table definition. Error: " + tableMembers.ErrorMessage;
                throw new ArgumentException(errMessage, nameof(tokens));
            }

            var columns = new List<Column>();
            var primaryKey = Option<PrimaryKey>.None;
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

                if (primaryKey.IsNone && parsedColumn.PrimaryKey.IsSome)
                    primaryKey = parsedColumn.PrimaryKey;

                parsedColumn.UniqueKey.IfSome(uk => uniqueKeys.Add(uk));
                foreignKeys.AddRange(parsedColumn.ForeignKeys);
                checks.AddRange(parsedColumn.Checks);
            }

            foreach (var parsedConstraint in parsedConstraints)
            {
                switch (parsedConstraint.ConstraintType)
                {
                    case TableConstraint.TableConstraintType.PrimaryKey:
                        if (parsedConstraint is TableConstraint.PrimaryKey pk)
                            primaryKey = new PrimaryKey(pk.Name, pk.Columns);
                        break;
                    case TableConstraint.TableConstraintType.UniqueKey:
                        if (parsedConstraint is TableConstraint.UniqueKey uk)
                            uniqueKeys.Add(new UniqueKey(uk.Name, uk.Columns));
                        break;
                    case TableConstraint.TableConstraintType.ForeignKey:
                        if (parsedConstraint is TableConstraint.ForeignKey fk)
                            foreignKeys.Add(new ForeignKey(fk.Name, fk.Columns, fk.ParentTable, fk.ParentColumnNames));
                        break;
                    case TableConstraint.TableConstraintType.Check:
                        if (parsedConstraint is TableConstraint.Check ck)
                            checks.Add(new Check(ck.Name, ck.Definition));
                        break;
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
    }
}
