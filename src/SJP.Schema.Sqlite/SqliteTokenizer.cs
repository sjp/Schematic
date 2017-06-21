using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schema.Core;

namespace SJP.Schema.Sqlite.Parsing
{
    public class SqliteTokenizer : Tokenizer<SqlToken>
    {
        protected override IEnumerable<Result<SqlToken>> Tokenize(TextSpan span)
        {
            var next = SkipWhiteSpace(span);
            if (!next.HasValue)
                yield break;

            do
            {
                if (next.Value.IsLetter())
                {
                    var typeName = TypeName(next.Location);
                    var op = NamedOperators(next.Location);
                    var literal = SqlLiteral(next.Location);
                    if (typeName.HasValue)
                    {
                        yield return Result.Value(SqlToken.Type, typeName.Location, typeName.Remainder);
                        next = typeName.Remainder.ConsumeChar();
                    }
                    else if (op.HasValue)
                    {
                        yield return Result.Value(SqlToken.Operator, op.Location, op.Remainder);
                        next = op.Remainder.ConsumeChar();
                    }
                    else if (literal.HasValue)
                    {
                        yield return Result.Value(SqlToken.Literal, literal.Location, literal.Remainder);
                        next = literal.Remainder.ConsumeChar();
                    }
                    else
                    {
                        var identifier = SqlIdentifier(next.Location);
                        if (identifier.HasValue)
                        {
                            var resultTokenType = SqlKeywords.Contains(identifier.Value) ? SqlToken.Keyword : SqlToken.Identifier;
                            yield return Result.Value(resultTokenType, identifier.Location, identifier.Remainder);
                            next = identifier.Remainder.ConsumeChar();
                        }
                        else
                        {
                            yield return Result.Empty<SqlToken>(next.Location);
                        }
                    }
                }
                else if (next.Value.IsDigit())
                {
                    var literal = SqlNumeric(next.Location);
                    if (literal.HasValue)
                    {
                        yield return Result.Value(SqlToken.Literal, literal.Location, literal.Remainder);
                        next = literal.Remainder.ConsumeChar();
                    }
                    else
                    {
                        yield return Result.Empty<SqlToken>(next.Location);
                    }
                }
                else if (next.Value == '[' || next.Value == '`' || next.Value == '"')
                {
                    var identifier = SqlIdentifier(next.Location);
                    if (identifier.HasValue)
                    {
                        yield return Result.Value(SqlToken.Identifier, identifier.Location, identifier.Remainder);
                        next = identifier.Remainder.ConsumeChar();
                    }
                    else
                    {
                        yield return Result.Empty<SqlToken>(next.Location);
                    }
                }
                else if (next.Value == '.')
                {
                    yield return Result.Value(SqlToken.Dot, next.Location, next.Remainder);
                    next = next.Remainder.ConsumeChar();
                }
                else if (next.Value == '(')
                {
                    yield return Result.Value(SqlToken.LParen, next.Location, next.Remainder);
                    next = next.Remainder.ConsumeChar();
                }
                else if (next.Value == ')')
                {
                    yield return Result.Value(SqlToken.RParen, next.Location, next.Remainder);
                    next = next.Remainder.ConsumeChar();
                }
                else if (next.Value == ',')
                {
                    yield return Result.Value(SqlToken.Delimiter, next.Location, next.Remainder);
                    next = next.Remainder.ConsumeChar();
                }
                else if (next.Value == '\'')
                {
                    var sqlString = SqlString(next.Location);
                    yield return sqlString.HasValue
                        ? Result.Value(SqlToken.Literal, sqlString.Location, sqlString.Remainder)
                        : Result.Empty<SqlToken>(next.Location);

                    next = sqlString.Remainder.ConsumeChar();
                }
                else if (next.Value == '-' || next.Value == '/')
                {
                    var sqlComment = SqlComment(next.Location);
                    if (sqlComment.HasValue)
                    {
                        // don't return comments, assume they're filtered out as it makes parsing more difficult
                        // yield return Result.Value(SqlToken.Comment, sqlComment.Location, sqlComment.Remainder);
                        next = sqlComment.Remainder.ConsumeChar();
                    }
                    else
                    {
                        var op = Operators(next.Location);
                        if (op.HasValue)
                        {
                            yield return Result.Value(SqlToken.Operator, next.Location, next.Remainder);
                            next = op.Remainder.ConsumeChar();
                        }
                        else
                        {
                            yield return Result.Empty<SqlToken>(next.Location);
                        }
                    }
                }
                else if (next.Value == ';')
                {
                    yield return Result.Value(SqlToken.Terminator, next.Location, next.Remainder);
                    next = next.Remainder.ConsumeChar();
                }
                else
                {
                    next = next.Remainder.ConsumeChar();
                }

                next = SkipWhiteSpace(next.Location);
            }
            while (next.HasValue);
        }

        private static TextParser<char> SqlStringContentChar =>
           Span.EqualTo("''").Value('\'').Try().Or(Character.ExceptIn('\'', '\r', '\n'));

        private static TextParser<string> SqlString =>
            Character.EqualTo('\'')
                .Then(prefix => SqlStringContentChar.Many().Select(chars => prefix.ToString() + new string(chars)))
                .Then(s => Character.EqualTo('\'').Value(s + "'"));

        private static TextParser<char> SqlInlineCommentChar =>
            Character.ExceptIn('\r', '\n');

        private static TextParser<string> SqlInlineComment =>
            Span.EqualTo("--")
                .Then(prefix => SqlInlineCommentChar.Many().Select(chars => prefix.ToString() + new string(chars)));

        private static TextParser<string> SqlBlockComment =>
            Span.EqualTo("/*")
                .Then(prefix => Character.AnyChar.Many().Select(chars => prefix.ToString() + new string(chars)))
                .Then(s => Span.EqualTo("*/").Value(s + "*/"));

        private static TextParser<string> SqlComment =>
            SqlInlineComment.Or(SqlBlockComment);

        private static TextParser<string> QuotedIdentifier =>
            Character.EqualTo('"')
                .IgnoreThen(Span.EqualTo("\"\"").Value('"').Try().Or(Character.Except('"')).Many().Select(chars => new string(chars)))
                .Then(s => Character.EqualTo('"').Value(s));

        private static TextParser<string> BracketedIdentifier =>
            Character.EqualTo('[')
                .IgnoreThen(Span.EqualTo("]]").Value(']').Try().Or(Character.AnyChar).Many().Select(chars => new string(chars)))
                .Then(s => Character.EqualTo(']').Value(s + "]"));

        private static TextParser<string> EngravedIdentifier =>
            Character.EqualTo('`')
                .IgnoreThen(Span.EqualTo("``").Value('`').Try().Or(Character.AnyChar).Many().Select(chars => new string(chars)))
                .Then(s => Character.EqualTo('`').Value(s));

        private static TextParser<char> SqlIdentifierStartChar =>
            Character.Letter.Or(Character.EqualTo('_'));

        private static TextParser<char> SqlIdentifierChar =>
            Character.LetterOrDigit.Or(Character.EqualTo('_'));

        private static TextParser<string> SqlSimpleIdentifier =>
            SqlIdentifierStartChar.AtLeastOnce()
                .Then(start =>
                    SqlIdentifierChar.Many()
                        .Select(suffix => new string(start) + new string(suffix))
                        .Try().Or(Parse.Return(new string(start))));

        private static TextParser<string> SqlIdentifier =>
            QuotedIdentifier
                .Or(BracketedIdentifier)
                .Or(EngravedIdentifier)
                .Or(SqlSimpleIdentifier);

        private static TextParser<string> SqlBlob =>
            Span.EqualToIgnoreCase("x")
                .Then(x => SqlString.Select(str => x.ToStringValue() + str));

        private static TextParser<string> SignedNumber =>
            Character.Matching(c => c == '+' || c == '-', "sign").Try()
                .Then(c => Character.Digit.AtLeastOnce().Select(digits => c.ToString() + new string(digits)));

        public static TextParser<string> SqlNumeric =>
            HexLiteral
                .Or(
                    DecimalPoint
                        .Then(point => Digit.AtLeastOnce().Select(digits => point + new string(digits)))
                        .Then(_ => ExponentSuffix.Select(suffix => _ + suffix).Try().Or(Parse.Return(_)))
                )
                .Or(
                    Digit.AtLeastOnce()
                        .Then(digits =>
                            DecimalPoint
                                .Select(p => new string(digits) + p.ToString())
                                .Then(prefix => Digit.Many()
                                    .Select(suffixDigits => prefix + new string(suffixDigits)))
                                .Try().Or(Parse.Return(new string(digits)))
                        )
                        .Then(_ => ExponentSuffix.Select(suffix => _ + suffix).Try().Or(Parse.Return(_)))
                );

        private static TextParser<string> SqlLiteral =>
            Span.EqualToIgnoreCase("NULL").Select(_ => _.ToStringValue())
                .Or(Span.EqualToIgnoreCase("CURRENT_TIME").Select(_ => _.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("CURRENT_DATE").Select(_ => _.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("CURRENT_TIMESTAMP").Select(_ => _.ToStringValue()))
                .Or(SqlString)
                .Or(SqlBlob)
                .Or(SqlNumeric);

        private static TextParser<char> Digit => Character.Digit;

        private static TextParser<char> DecimalPoint => Character.EqualTo('.');

        private readonly static ISet<char> _hexLetters = new HashSet<char> { 'A', 'B', 'C', 'D', 'E', 'F' };

        private static TextParser<char> HexDigit =>
            Character.Matching(c => c.IsDigit() || _hexLetters.Contains(c.ToUpperInvariant()), "hex digit");

        private static TextParser<string> HexPrefix =>
            Span.EqualTo("0x").Value("0x");

        private static TextParser<string> HexLiteral =>
            HexPrefix
                .Then(prefix => HexDigit.AtLeastOnce().Select(digits => prefix + new string(digits)));

        private static TextParser<string> ExponentSuffix =>
            Character.EqualTo('E')
                .Then(e => Character.Matching(c => c == '+' || c == '-', "sign")
                    .Select(sign => e.ToString() + sign.ToString())
                    .Try().Or(Parse.Return(e.ToString()))
                )
                .Then(prefix => Character.Digit.AtLeastOnce().Select(digits => prefix + new string(digits)));

        private static TextParser<string> RaiseFunction =>
            Span.EqualToIgnoreCase("RAISE")
                .Then(r => Span.EqualTo('(').Select(paren => r.ToStringValue() + paren.ToStringValue()))
                .Then(prefix =>
                    Span.EqualToIgnoreCase("IGNORE").Select(ign => prefix + ign.ToStringValue())
                    .Or(
                            Span.EqualToIgnoreCase("ROLLBACK")
                        .Or(Span.EqualToIgnoreCase("ABORT"))
                        .Or(Span.EqualToIgnoreCase("FAIL"))
                        .Then(keyword => Span.EqualTo(",").Select(comma => keyword.ToStringValue() + comma.ToStringValue()))
                        .Then(main => SqlString.Select(errmsg => main + errmsg))
                    )
                    .Then(main => Span.EqualTo(")").Select(paren => main + paren.ToStringValue()))
                )
                .Then(body => Span.EqualTo(")").Select(end => body + end.ToStringValue()));

        private static TextParser<string> QualifedIdentifier =>
            SqlIdentifier // maybe schema name
                .Then(ident => Span.EqualTo(".").Select(dot => ident + dot.ToStringValue()).Try().Or(Parse.Return(ident)))
                .Then(ident => SqlLiteral.Select(lit => ident + lit).Try().Or(Parse.Return(ident))) // maybe table name
                .Then(ident => Span.EqualTo(".").Select(dot => ident + dot.ToStringValue()).Try().Or(Parse.Return(ident)))
                .Then(ident => SqlLiteral.Select(lit => ident + lit).Try().Or(Parse.Return(ident))); // maybe column name

        private static TextParser<string> NamedOperators =>
            Span.EqualToIgnoreCase("REGEXP").Select(op => op.ToStringValue())
                .Or(Span.EqualToIgnoreCase("MATCH").Select(op => op.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("GLOB").Select(op => op.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("LIKE").Select(op => op.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("AND").Select(op => op.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("NOT").Select(op => op.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("IN").Select(op => op.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("IS").Select(op => op.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("OR").Select(op => op.ToStringValue()));
        private static TextParser<string> Operators =>
            Span.EqualToIgnoreCase("REGEXP").Select(op => op.ToStringValue())
                .Or(Span.EqualToIgnoreCase("MATCH").Select(op => op.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("GLOB").Select(op => op.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("LIKE").Select(op => op.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("AND").Select(op => op.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("NOT").Select(op => op.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("IN").Select(op => op.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("IS").Select(op => op.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("OR").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo("||").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo("<<").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo(">>").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo("<>").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo("!=").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo("==").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo(">=").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo("<=").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo("*").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo("/").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo("%").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo("+").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo("-").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo("&").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo("|").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo("<").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo(">").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo("=").Select(op => op.ToStringValue()))
                .Or(Span.EqualTo("~").Select(op => op.ToStringValue()));

        private static TextParser<string> TypeName =>
            // TEXT
            Span.EqualToIgnoreCase("CLOB").Select(typeName => typeName.ToStringValue())
                .Or(Span.EqualToIgnoreCase("TEXT").Select(typeName => typeName.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("NVARCHAR").Select(typeName => typeName.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("NCHAR").Select(typeName => typeName.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("VARCHAR").Select(typeName => typeName.ToStringValue()))
                .Or(
                    Span.EqualToIgnoreCase("VARYING").Select(typeName => typeName.ToStringValue())
                        .Then(dbl => Character.WhiteSpace.AtLeastOnce().Select(ws => dbl + new string(ws)))
                        .Then(prefix => Span.EqualToIgnoreCase("CHARACTER").Select(typeName => prefix + typeName.ToStringValue()))
                )
                .Or(
                    Span.EqualToIgnoreCase("NATIVE").Select(typeName => typeName.ToStringValue())
                        .Then(dbl => Character.WhiteSpace.AtLeastOnce().Select(ws => dbl + new string(ws)))
                        .Then(prefix => Span.EqualToIgnoreCase("CHARACTER").Select(typeName => prefix + typeName.ToStringValue()))
                )
                .Or(Span.EqualToIgnoreCase("CHARACTER").Select(typeName => typeName.ToStringValue()))
                // INTEGER
                .Or(Span.EqualToIgnoreCase("INTEGER").Select(typeName => typeName.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("TINYINT").Select(typeName => typeName.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("SMALLINT").Select(typeName => typeName.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("MEDIUMINT").Select(typeName => typeName.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("BIGINT").Select(typeName => typeName.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("INT8").Select(typeName => typeName.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("INT2").Select(typeName => typeName.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("INT").Select(typeName => typeName.ToStringValue()))
                .Or(
                    Span.EqualToIgnoreCase("UNSIGNED").Select(typeName => typeName.ToStringValue())
                        .Then(unsign => Character.WhiteSpace.AtLeastOnce().Select(ws => unsign + new string(ws)))
                        .Then(prefix => Span.EqualToIgnoreCase("BIG").Select(typeName => prefix + typeName.ToStringValue()))
                        .Then(prefix => Character.WhiteSpace.AtLeastOnce().Select(ws => prefix + new string(ws)))
                        .Then(prefix => Span.EqualToIgnoreCase("INT").Select(typeName => prefix + typeName.ToStringValue()))
                )

                // BLOB
                .Or(Span.EqualToIgnoreCase("BLOB").Select(typeName => typeName.ToStringValue()))
                // REAL
                .Or(
                    Span.EqualToIgnoreCase("DOUBLE").Select(typeName => typeName.ToStringValue())
                        .Then(dbl => Character.WhiteSpace.AtLeastOnce().Select(ws => dbl + new string(ws)))
                        .Then(prefix => Span.EqualToIgnoreCase("PRECISION").Select(typeName => prefix + typeName.ToStringValue()))
                )
                .Or(Span.EqualToIgnoreCase("DOUBLE").Select(typeName => typeName.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("REAL").Select(typeName => typeName.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("FLOAT").Select(typeName => typeName.ToStringValue()))
                // numeric
                .Or(Span.EqualToIgnoreCase("NUMERIC").Select(typeName => typeName.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("DECIMAL").Select(typeName => typeName.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("BOOLEAN").Select(typeName => typeName.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("DATETIME").Select(typeName => typeName.ToStringValue()))
                .Or(Span.EqualToIgnoreCase("DATE").Select(typeName => typeName.ToStringValue()));

        private static ISet<string> SqlKeywords { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ABORT",
            "ACTION",
            "ADD",
            "AFTER",
            "ALL",
            "ALTER",
            "ANALYZE",
            "AND",
            "AS",
            "ASC",
            "ATTACH",
            "AUTOINCREMENT",
            "BEFORE",
            "BEGIN",
            "BETWEEN",
            "BY",
            "CASCADE",
            "CASE",
            "CAST",
            "CHECK",
            "COLLATE",
            "COLUMN",
            "COMMIT",
            "CONFLICT",
            "CONSTRAINT",
            "CREATE",
            "CROSS",
            "CURRENT_DATE",
            "CURRENT_TIME",
            "CURRENT_TIMESTAMP",
            "DATABASE",
            "DEFAULT",
            "DEFERRABLE",
            "DEFERRED",
            "DELETE",
            "DESC",
            "DETACH",
            "DISTINCT",
            "DROP",
            "EACH",
            "ELSE",
            "END",
            "ESCAPE",
            "EXCEPT",
            "EXCLUSIVE",
            "EXISTS",
            "EXPLAIN",
            "FAIL",
            "FOR",
            "FOREIGN",
            "FROM",
            "FULL",
            "GLOB",
            "GROUP",
            "HAVING",
            "IF",
            "IGNORE",
            "IMMEDIATE",
            "IN",
            "INDEX",
            "INDEXED",
            "INITIALLY",
            "INNER",
            "INSERT",
            "INSTEAD",
            "INTERSECT",
            "INTO",
            "IS",
            "ISNULL",
            "JOIN",
            "KEY",
            "LEFT",
            "LIKE",
            "LIMIT",
            "MATCH",
            "NATURAL",
            "NO",
            "NOT",
            "NOTNULL",
            "NULL",
            "OF",
            "OFFSET",
            "ON",
            "OR",
            "ORDER",
            "OUTER",
            "PLAN",
            "PRAGMA",
            "PRIMARY",
            "QUERY",
            "RAISE",
            "RECURSIVE",
            "REFERENCES",
            "REGEXP",
            "REINDEX",
            "RELEASE",
            "RENAME",
            "REPLACE",
            "RESTRICT",
            "RIGHT",
            "ROLLBACK",
            "ROW",
            "SAVEPOINT",
            "SELECT",
            "SET",
            "TABLE",
            "TEMP",
            "TEMPORARY",
            "THEN",
            "TO",
            "TRANSACTION",
            "TRIGGER",
            "UNION",
            "UNIQUE",
            "UPDATE",
            "USING",
            "VACUUM",
            "VALUES",
            "VIEW",
            "VIRTUAL",
            "WHEN",
            "WHERE",
            "WITH",
            "WITHOUT"
        };
    }
}
