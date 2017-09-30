using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using EnumsNET;

namespace SJP.Schematic.Sqlite.Parsing
{
    public class SqliteTableParser
    {
        public SqliteTableParser(TokenList<SqlToken> tokens)
        {
            if (tokens == default(TokenList<SqlToken>) || tokens.Empty())
                throw new ArgumentNullException(nameof(tokens));

            var parseResult = ParseTokens(tokens);
            Columns = parseResult.Columns.ToList();
            Constraints = Columns
                .SelectMany(col => col.Constraints)
                .Concat(parseResult.Constraints)
                .ToList();
        }

        public IEnumerable<Column> Columns { get; set; }

        public IEnumerable<Constraint> Constraints { get; set; }

        private static ParseResult ParseTokens(TokenList<SqlToken> tokens)
        {
            var columns = new List<Column>();
            var constraints = new List<Constraint>();

            var next = tokens.ConsumeToken();
            while (next.Value.Kind != SqlToken.LParen || (next.Value.Kind == SqlToken.Keyword && next.Value.Span.EqualsValueIgnoreCase("AS")))
                next = next.Remainder.ConsumeToken();
            if (next.Value.Kind == SqlToken.Keyword && next.Value.Span.EqualsValueIgnoreCase("AS"))
                return ParseResult.Empty;

            // now at the start of the create table with proper column lists
            // i.e. not a select-based created table
            do
            {
                var kind = next.Value.Kind;
                var span = next.Value.Span;

                if (kind == SqlToken.Identifier)
                {
                    var result = ParseColumn(next.Location);
                    columns.Add(result.Item1);
                    next = result.Item2;
                }
                else if (kind == SqlToken.Keyword && _constraintStartWords.Contains(span.ToStringValue()))
                {
                    var result = ParseConstraint(next.Location);
                    constraints.Add(result.Item1);
                    next = result.Item2;
                }

                next = next.Remainder.ConsumeToken();
            }
            while (!next.Remainder.IsAtEnd);

            return new ParseResult(columns, constraints);
        }

        private class ParseResult
        {
            public ParseResult(IEnumerable<Column> columns, IEnumerable<Constraint> constraints)
            {
                if (columns == null)
                    throw new ArgumentNullException(nameof(columns));
                if (constraints == null)
                    throw new ArgumentNullException(nameof(constraints));

                Columns = columns.ToList();
                Constraints = constraints.ToList();
            }

            public IEnumerable<Column> Columns { get; }

            public IEnumerable<Constraint> Constraints { get; }

            public static ParseResult Empty => new ParseResult(Enumerable.Empty<Column>(), Enumerable.Empty<Constraint>());
        }

        private static Tuple<Column, TokenListParserResult<SqlToken, Token<SqlToken>>> ParseColumn(TokenList<SqlToken> tokens)
        {
            var startToken = tokens.ConsumeToken();
            var columnName = UnwrapIdentifier(startToken.Value.ToStringValue());
            string typeName = null;
            var collation = SqliteCollation.None;
            bool isAutoIncrement = false;
            string foreignKeyTableName = null;
            string constraintName = null;

            var next = startToken.Remainder.ConsumeToken();
            if (next.Value.Kind == SqlToken.Type)
            {
                typeName = next.Value.ToStringValue();
                next = next.Remainder.ConsumeToken();
            }

            var columnConstraints = new List<Constraint>();
            if (next.Value.Kind == SqlToken.RParen || next.Value.Kind == SqlToken.Delimiter)
            {
                var col = new Column(columnName, typeName, collation, isAutoIncrement, columnConstraints);
                return new Tuple<Column, TokenListParserResult<SqlToken, Token<SqlToken>>>(col, next);
            }

            // parse the constraints on the column
            while (next.Value.Kind != SqlToken.RParen && next.Value.Kind != SqlToken.Delimiter)
            {
                var consName = ConstraintName(next.Location);
                if (consName.HasValue)
                {
                    constraintName = UnwrapIdentifier(consName.Value);
                    next = consName.Remainder.ConsumeToken();
                    continue;
                }

                var hasPrimary = Primary(next.Location);
                if (hasPrimary.HasValue)
                {
                    var autoInc = PrimaryAutoIncrement(next.Location);
                    if (autoInc.HasValue)
                        isAutoIncrement = true;

                    next = autoInc.HasValue
                        ? autoInc.Remainder.ConsumeToken()
                        : hasPrimary.Remainder.ConsumeToken();

                    var pkConstraint = new Constraint(constraintName, ConstraintType.Primary, new[] { columnName });
                    columnConstraints.Add(pkConstraint);
                    constraintName = null;
                    continue;
                }

                var hasUnique = Unique(next.Location);
                if (hasUnique.HasValue)
                {
                    next = hasUnique.Remainder.ConsumeToken();

                    var ukConstraint = new Constraint(constraintName, ConstraintType.Unique, new[] { columnName });
                    columnConstraints.Add(ukConstraint);
                    constraintName = null;
                    continue;
                }

                var hasCheck = Check(next.Location);
                if (hasCheck.HasValue)
                {
                    var checkExpression = hasCheck.Value;
                    next = hasCheck.Remainder.ConsumeToken();

                    var ckConstraint = new Constraint(constraintName, ConstraintType.Check, new[] { columnName }, tokens: checkExpression);
                    columnConstraints.Add(ckConstraint);
                    constraintName = null;
                    continue;
                }

                var hasNotNull = NotNull(next.Location);
                if (hasNotNull.HasValue)
                {
                    next = hasNotNull.Remainder.ConsumeToken();
                    constraintName = null;
                    continue;
                }

                var hasDefaultValue = Default(next.Location);
                if (hasDefaultValue.HasValue)
                {
                    next = hasDefaultValue.Remainder.ConsumeToken();

                    var isLiteral = SignedNumberOrLiteral(next.Location);
                    if (isLiteral.HasValue)
                    {
                        next = isLiteral.Remainder.ConsumeToken();
                        constraintName = null;
                        continue;
                    }

                    var isExpression = Expression(next.Location);
                    if (isExpression.HasValue)
                    {
                        next = isExpression.Remainder.ConsumeToken();
                        constraintName = null;
                        continue;
                    }
                }

                var collateName = CollationName(next.Location);
                if (collateName.HasValue)
                {
                    if (!Enum.TryParse(collateName.Value, out collation))
                        collation = SqliteCollation.None;

                    next = collateName.Remainder.ConsumeToken();
                    constraintName = null;
                    continue;
                }

                var hasForeign = Foreign(next.Location);
                if (hasForeign.HasValue)
                {
                    var foreignTable = QualifiedName(hasForeign.Remainder);
                    if (foreignTable.HasValue)
                        foreignKeyTableName = UnwrapIdentifier(foreignTable.Value);

                    var foreignKeyColumns = Enumerable.Empty<string>();
                    var fkColumns = ForeignKeyColumns(foreignTable.Remainder);
                    if (fkColumns.HasValue)
                        foreignKeyColumns = fkColumns.Value.Select(UnwrapIdentifier);

                    next = fkColumns.Remainder.ConsumeToken();

                    var fkConstraint = new Constraint(constraintName, ConstraintType.Foreign, new[] { columnName }, foreignKeyTableName, foreignKeyColumns);
                    columnConstraints.Add(fkConstraint);
                    constraintName = null;
                    continue;
                }

                next = next.Remainder.ConsumeToken();
            }

            // https://sqlite.org/lang_createtable.html says that any undeclared column type is "NONE", i.e. the old name for "BLOB"
            typeName = typeName ?? "BLOB";

            var column = new Column(columnName, typeName, collation, isAutoIncrement, columnConstraints);
            return new Tuple<Column, TokenListParserResult<SqlToken, Token<SqlToken>>>(column, next);
        }

        private static string UnwrapIdentifier(string input)
        {
            if (input.StartsWith("\""))
            {
                var result = TrimEndChars(input);
                return result.Replace("\"\"", "\"");
            }
            else if (input.StartsWith("["))
            {
                var result = TrimEndChars(input);
                return result.Replace("]]", "]");
            }
            else if (input.StartsWith("`"))
            {
                var result = TrimEndChars(input);
                return result.Replace("``", "`");
            }
            else
            {
                return input;
            }
        }

        private static string TrimEndChars(string input)
        {
            return input.Substring(1, input.Length - 2);
        }

        private static Tuple<Constraint, TokenListParserResult<SqlToken, Token<SqlToken>>> ParseConstraint(TokenList<SqlToken> tokens)
        {
            var next = tokens.ConsumeToken();
            string constraintName = null;

            while (next.Value.Kind != SqlToken.RParen && next.Value.Kind != SqlToken.Delimiter)
            {
                var consName = ConstraintName(next.Location);
                if (consName.HasValue)
                {
                    constraintName = UnwrapIdentifier(consName.Value);
                    next = consName.Remainder.ConsumeToken();
                    continue;
                }

                var hasPrimary = ConstraintPrimaryKeyColumns(next.Location);
                if (hasPrimary.HasValue)
                {
                    next = hasPrimary.Remainder.ConsumeToken();
                    var columnNames = hasPrimary.Value.Select(UnwrapIdentifier);
                    var pkConstraint = new Constraint(constraintName, ConstraintType.Primary, columnNames);
                    return new Tuple<Constraint, TokenListParserResult<SqlToken, Token<SqlToken>>>(pkConstraint, next);
                }

                var hasUnique = ConstraintUniqueKeyColumns(next.Location);
                if (hasUnique.HasValue)
                {
                    var columnNames = hasUnique.Value.Select(UnwrapIdentifier);
                    next = hasUnique.Remainder.ConsumeToken();

                    var ukConstraint = new Constraint(constraintName, ConstraintType.Unique, columnNames);
                    return new Tuple<Constraint, TokenListParserResult<SqlToken, Token<SqlToken>>>(ukConstraint, next);
                }

                var hasCheck = Check(next.Location);
                if (hasCheck.HasValue)
                {
                    var checkExpression = hasCheck.Value;
                    next = hasCheck.Remainder.ConsumeToken();

                    var ckConstraint = new Constraint(constraintName, ConstraintType.Check, Enumerable.Empty<string>(), tokens: checkExpression);
                    return new Tuple<Constraint, TokenListParserResult<SqlToken, Token<SqlToken>>>(ckConstraint, next);
                }

                var hasForeign = ConstraintForeignKeyColumns(next.Location);
                if (hasForeign.HasValue)
                {
                    var sourceColumns = hasForeign.Value.Select(UnwrapIdentifier);
                    next = hasForeign.Remainder.ConsumeToken();

                    var references = Foreign(next.Location);
                    next = references.Remainder.ConsumeToken();

                    string foreignKeyTableName = null;
                    var foreignTable = QualifiedName(references.Remainder);
                    if (foreignTable.HasValue)
                        foreignKeyTableName = UnwrapIdentifier(foreignTable.Value);

                    var foreignKeyColumns = Enumerable.Empty<string>();
                    var fkColumns = ForeignKeyColumns(foreignTable.Remainder);
                    if (fkColumns.HasValue)
                        foreignKeyColumns = fkColumns.Value.Select(UnwrapIdentifier);

                    next = fkColumns.Remainder.ConsumeToken();
                    var fkConstraint = new Constraint(constraintName, ConstraintType.Foreign, sourceColumns, foreignKeyTableName, foreignKeyColumns);
                    return new Tuple<Constraint, TokenListParserResult<SqlToken, Token<SqlToken>>>(fkConstraint, next);
                }

                next = next.Remainder.ConsumeToken();
            }

            throw new Exception("Could not parse a constraint for the table.");
        }

        private static TokenListParser<SqlToken, IEnumerable<string>> ConstraintForeignKeyColumns =>
            Token.EqualToValueIgnoreCase(SqlToken.Keyword, "FOREIGN")
                .IgnoreThen(Token.EqualToValueIgnoreCase(SqlToken.Keyword, "KEY"))
                .IgnoreThen(Token.EqualTo(SqlToken.LParen))
                .IgnoreThen(
                    Token.EqualTo(SqlToken.Identifier)
                        .AtLeastOnceDelimitedBy(Token.EqualTo(SqlToken.Delimiter))
                )
                .Then(_ => Token.EqualTo(SqlToken.RParen).Select(paren => _.Select(col => col.ToStringValue())));

        private static TokenListParser<SqlToken, IEnumerable<string>> ConstraintUniqueKeyColumns =>
            Token.EqualToValueIgnoreCase(SqlToken.Keyword, "UNIQUE")
                .IgnoreThen(Token.EqualTo(SqlToken.LParen))
                .IgnoreThen(
                    Token.EqualTo(SqlToken.Identifier)
                        .AtLeastOnceDelimitedBy(Token.EqualTo(SqlToken.Delimiter))
                )
                .Then(_ => Token.EqualTo(SqlToken.RParen).Select(paren => _.Select(col => col.ToStringValue())));

        private static TokenListParser<SqlToken, IEnumerable<string>> ConstraintPrimaryKeyColumns =>
            Token.EqualToValueIgnoreCase(SqlToken.Keyword, "PRIMARY")
                .IgnoreThen(Token.EqualToValueIgnoreCase(SqlToken.Keyword, "KEY"))
                .IgnoreThen(Token.EqualTo(SqlToken.LParen))
                .IgnoreThen(
                    Token.EqualTo(SqlToken.Identifier)
                        .AtLeastOnceDelimitedBy(Token.EqualTo(SqlToken.Delimiter))
                )
                .Then(_ => Token.EqualTo(SqlToken.RParen).Select(paren => _.Select(col => col.ToStringValue())));

        private static TokenListParser<SqlToken, IEnumerable<Token<SqlToken>>> Expression =>
            Token.EqualTo(SqlToken.LParen)
                .Then(_ =>
                    Expression
                        .Select(expr => new[] { _ }.Concat(expr))
                        .Or(
                            Token.EqualTo(SqlToken.Keyword)
                                .Or(Token.EqualTo(SqlToken.None))
                                .Or(Token.EqualTo(SqlToken.Keyword))
                                .Or(Token.EqualTo(SqlToken.Identifier))
                                .Or(Token.EqualTo(SqlToken.Delimiter))
                                .Or(Token.EqualTo(SqlToken.Dot))
                                .Or(Token.EqualTo(SqlToken.Comment))
                                .Or(Token.EqualTo(SqlToken.Literal))
                                .Or(Token.EqualTo(SqlToken.Operator))
                                .Or(Token.EqualTo(SqlToken.Terminator))
                                .Or(Token.EqualTo(SqlToken.Type))
                            .Many()
                            .Select(tokens => new[] { _ }.Concat(tokens))
                        )
                )
                // once more because we might obtain valid values from within an expression then encounter a nested expression
                .Then(_ =>
                    Expression
                        .Select(_.Concat)
                        .Or(
                            Token.EqualTo(SqlToken.Keyword)
                                .Or(Token.EqualTo(SqlToken.None))
                                .Or(Token.EqualTo(SqlToken.Keyword))
                                .Or(Token.EqualTo(SqlToken.Identifier))
                                .Or(Token.EqualTo(SqlToken.Delimiter))
                                .Or(Token.EqualTo(SqlToken.Dot))
                                .Or(Token.EqualTo(SqlToken.Comment))
                                .Or(Token.EqualTo(SqlToken.Literal))
                                .Or(Token.EqualTo(SqlToken.Operator))
                                .Or(Token.EqualTo(SqlToken.Terminator))
                                .Or(Token.EqualTo(SqlToken.Type))
                            .Many()
                            .Select(tokens => _.Concat(tokens))
                        )
                ).Try()
                .Then(prefix =>
                    Token.EqualTo(SqlToken.RParen)
                        .Select(end => prefix.Concat(new List<Token<SqlToken>> { end }))
                );

        private readonly static IEnumerable<string> _constraintStartWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CONSTRAINT",
            "PRIMARY",
            "UNIQUE",
            "CHECK",
            "FOREIGN"
        };

        private static TokenListParser<SqlToken, string> CollationName =>
            Token.EqualToValueIgnoreCase(SqlToken.Keyword, "COLLATE")
                .IgnoreThen(
                    Token.EqualToValueIgnoreCase(SqlToken.Identifier, "BINARY").Value("BINARY")
                        .Or(Token.EqualToValueIgnoreCase(SqlToken.Identifier, "NOCASE").Value("NOCASE"))
                        .Or(Token.EqualToValueIgnoreCase(SqlToken.Identifier, "RTRIM").Value("RTRIM"))
                );

        private static TokenListParser<SqlToken, string> Default =>
            Token.EqualToValueIgnoreCase(SqlToken.Keyword, "DEFAULT").Select(_ => _.ToStringValue());

        private static TokenListParser<SqlToken, string> NotNull =>
            Token.EqualToValueIgnoreCase(SqlToken.Keyword, "NOT")
                .IgnoreThen(Token.EqualToValueIgnoreCase(SqlToken.Literal, "NULL"))
                .Select(_ => _.ToStringValue());

        private static TokenListParser<SqlToken, string> Unique =>
            Token.EqualToValueIgnoreCase(SqlToken.Keyword, "UNIQUE").Select(_ => _.ToStringValue());

        private static TokenListParser<SqlToken, string> Primary =>
            Token.EqualToValueIgnoreCase(SqlToken.Keyword, "PRIMARY")
                .IgnoreThen(Token.EqualToValueIgnoreCase(SqlToken.Keyword, "KEY").Select(_ => _.ToStringValue()));

        private static TokenListParser<SqlToken, IEnumerable<Token<SqlToken>>> Check =>
            Token.EqualToValueIgnoreCase(SqlToken.Keyword, "CHECK")
                .IgnoreThen(Expression);

        private static TokenListParser<SqlToken, string> Foreign =>
            Token.EqualToValueIgnoreCase(SqlToken.Keyword, "REFERENCES").Select(_ => _.ToStringValue());

        private static TokenListParser<SqlToken, IEnumerable<string>> ForeignKeyColumns =>
            Token.EqualTo(SqlToken.LParen).Value(string.Empty)
                .IgnoreThen(
                    Token.EqualTo(SqlToken.Identifier)
                        .AtLeastOnceDelimitedBy(Token.EqualTo(SqlToken.Delimiter))
                        .Select(tokens => tokens.Select(t => t.ToStringValue()))
                );

        private static TokenListParser<SqlToken, string> PrimaryAutoIncrement =>
            Primary
                .IgnoreThen(
                    Token.EqualToValueIgnoreCase(SqlToken.Keyword, "ASC").Value("ASC")
                        .Or(Token.EqualToValueIgnoreCase(SqlToken.Keyword, "DESC").Value("DESC"))
                        .Or(
                            ConflictClause
                                .IgnoreThen(Token.EqualToValueIgnoreCase(SqlToken.Keyword, "AUTOINCREMENT").Value("AUTOINCREMENT"))
                        )
                        .Or(Token.EqualToValueIgnoreCase(SqlToken.Keyword, "AUTOINCREMENT").Value("AUTOINCREMENT"))
                );

        private static TokenListParser<SqlToken, string> UniqueConflictClause =>
            Token.EqualToValueIgnoreCase(SqlToken.Keyword, "UNIQUE")
                .IgnoreThen(ConflictClause);

        private static TokenListParser<SqlToken, string> NotNullConflictClause =>
            Token.EqualToValueIgnoreCase(SqlToken.Keyword, "NOT")
                .IgnoreThen(Token.EqualToValueIgnoreCase(SqlToken.Literal, "NULL"))
                .IgnoreThen(ConflictClause);

        private static TokenListParser<SqlToken, string> ConflictClause =>
            Token.EqualToValueIgnoreCase(SqlToken.Keyword, "ON")
                .IgnoreThen(Token.EqualToValueIgnoreCase(SqlToken.Keyword, "CONFLICT"))
                .IgnoreThen(
                    Token.EqualToValueIgnoreCase(SqlToken.Keyword, "ROLLBACK").Select(_ => _.ToStringValue())
                        .Or(Token.EqualToValueIgnoreCase(SqlToken.Keyword, "ABORT").Select(_ => _.ToStringValue()))
                        .Or(Token.EqualToValueIgnoreCase(SqlToken.Keyword, "FAIL").Select(_ => _.ToStringValue()))
                        .Or(Token.EqualToValueIgnoreCase(SqlToken.Keyword, "IGNORE").Select(_ => _.ToStringValue()))
                        .Or(Token.EqualToValueIgnoreCase(SqlToken.Keyword, "REPLACE").Select(_ => _.ToStringValue()))
                );

        private static TokenListParser<SqlToken, string> ConstraintName =>
            Token.EqualToValueIgnoreCase(SqlToken.Keyword, "CONSTRAINT")
                .IgnoreThen(Token.EqualTo(SqlToken.Identifier).Select(ident => ident.ToStringValue()));

        private static TokenListParser<SqlToken, string> QualifiedName =>
            Token.EqualTo(SqlToken.Identifier)
                .IgnoreThen(Token.EqualTo(SqlToken.Dot))
                .IgnoreThen(Token.EqualTo(SqlToken.Identifier).Select(ident => ident.ToStringValue()))
                .IgnoreThen(Token.EqualTo(SqlToken.Dot))
                .IgnoreThen(Token.EqualTo(SqlToken.Identifier).Select(ident => ident.ToStringValue()))
            .Try().Or(
                Token.EqualTo(SqlToken.Identifier)
                    .IgnoreThen(Token.EqualTo(SqlToken.Dot))
                    .IgnoreThen(Token.EqualTo(SqlToken.Identifier).Select(ident => ident.ToStringValue()))
            ).Try().Or(Token.EqualTo(SqlToken.Identifier).Select(ident => ident.ToStringValue()));

        private static TokenListParser<SqlToken, string> SignedNumberOrLiteral =>
            Token.EqualTo(SqlToken.Operator)
                .Try()
                .IgnoreThen(Token.EqualTo(SqlToken.Literal).Select(_ => _.ToStringValue()));

        public enum SqliteCollation
        {
            None,
            Binary,
            NoCase,
            Rtrim
        }

        public class Column
        {
            public Column(string name, string typeName, SqliteCollation collation, bool isAutoIncrement, IEnumerable<Constraint> constraints)
            {
                if (!collation.IsValid())
                    throw new ArgumentException($"The { nameof(SqliteCollation) } provided must be a valid enum.", nameof(collation));
                if (constraints == null)
                    throw new ArgumentNullException(nameof(constraints));

                Name = name ?? throw new ArgumentNullException(nameof(name));
                TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName)); // no validation as can be null (i.e untyped)
                Collation = collation;
                IsAutoIncrement = isAutoIncrement;
                Constraints = constraints.ToList();
            }

            public string Name { get; }

            public string TypeName { get; }

            public SqliteCollation Collation { get; }

            public bool IsAutoIncrement { get; }

            public IEnumerable<Constraint> Constraints { get; }
        }

        public enum ConstraintType
        {
            None,
            Primary,
            Unique,
            Check,
            Foreign
        }

        public class Constraint
        {
            public Constraint(string name, ConstraintType type, IEnumerable<string> columns,
                string foreignKeyTableName = null, IEnumerable<string> foreignKeyColumns = null, IEnumerable<Token<SqlToken>> tokens = null)
            {
                if (!type.IsValid())
                    throw new ArgumentException($"The { nameof(ConstraintType) } provided must be a valid enum.", nameof(type));
                if (columns == null)
                    throw new ArgumentNullException(nameof(columns));

                Name = name;
                Type = type;
                Columns = columns.ToList();
                ForeignKeyTableName = foreignKeyTableName;
                foreignKeyColumns = foreignKeyColumns ?? Enumerable.Empty<string>();
                ForeignKeyColumns = foreignKeyColumns.ToList();
                Tokens = tokens != null ? tokens.ToList() : Enumerable.Empty<Token<SqlToken>>();
            }

            public string Name { get; }

            public ConstraintType Type { get; }

            public IEnumerable<string> Columns { get; }

            public string ForeignKeyTableName { get; }

            public IEnumerable<string> ForeignKeyColumns { get; }

            public IEnumerable<Token<SqlToken>> Tokens { get; }
        }
    }
}
