using System.Collections.Generic;
using SJP.Schematic.Core;
using Superpower;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing
{
    public class SqliteTokenizer : Tokenizer<SqliteToken>
    {
        static SqliteTokenizer()
        {
            SimpleOps['+'] = SqliteToken.Plus;
            SimpleOps['-'] = SqliteToken.Minus;
            SimpleOps['*'] = SqliteToken.Asterisk;
            SimpleOps['/'] = SqliteToken.ForwardSlash;
            SimpleOps['%'] = SqliteToken.Percent;
            SimpleOps['^'] = SqliteToken.Caret;
            SimpleOps['<'] = SqliteToken.LessThan;
            SimpleOps['>'] = SqliteToken.GreaterThan;
            SimpleOps['='] = SqliteToken.Equal;
            SimpleOps[','] = SqliteToken.Comma;
            SimpleOps['.'] = SqliteToken.Period;
            SimpleOps['('] = SqliteToken.LParen;
            SimpleOps[')'] = SqliteToken.RParen;
            SimpleOps['['] = SqliteToken.LBracket;
            SimpleOps[']'] = SqliteToken.RBracket;
            SimpleOps['?'] = SqliteToken.QuestionMark;
            SimpleOps['&'] = SqliteToken.Ampersand;
            SimpleOps['|'] = SqliteToken.Pipe;
            SimpleOps['~'] = SqliteToken.Tilde;
            SimpleOps['`'] = SqliteToken.Backtick;
            SimpleOps['"'] = SqliteToken.DoubleQuote;
            SimpleOps[';'] = SqliteToken.Semicolon;
        }

        protected override IEnumerable<Result<SqliteToken>> Tokenize(TextSpan span)
        {
            var next = SkipWhiteSpace(span);
            if (!next.HasValue)
                yield break;

            do
            {
                if (next.Value.IsDigit())
                {
                    var hex = SqliteTextParsers.HexInteger(next.Location);
                    if (hex.HasValue)
                    {
                        next = hex.Remainder.ConsumeChar();
                        yield return Result.Value(SqliteToken.HexNumber, hex.Location, hex.Remainder);
                    }
                    else
                    {
                        var real = SqliteTextParsers.Real(next.Location);
                        if (!real.HasValue)
                            yield return Result.CastEmpty<TextSpan, SqliteToken>(real);
                        else
                            yield return Result.Value(SqliteToken.Number, real.Location, real.Remainder);

                        next = real.Remainder.ConsumeChar();
                    }

                    if (next.HasValue && !next.Value.IsPunctuation() && !next.Value.IsWhiteSpace())
                    {
                        yield return Result.Empty<SqliteToken>(next.Location, new[] { "digit" });
                    }
                }
                else if (next.Value == '\'')
                {
                    var str = SqliteTextParsers.SqlString(next.Location);
                    if (!str.HasValue)
                        yield return Result.CastEmpty<string, SqliteToken>(str);

                    next = str.Remainder.ConsumeChar();

                    yield return Result.Value(SqliteToken.String, str.Location, str.Remainder);
                }
                else if (next.Value == '`' || next.Value == '"' || next.Value == '[')
                {
                    var endChar = next.Value == '[' ? ']' : next.Value;

                    var beginIdentifier = next.Location;
                    do
                    {
                        next = next.Remainder.ConsumeChar();
                    }
                    while (next.HasValue && (next.Value != endChar));

                    yield return Result.Value(SqliteToken.Identifier, beginIdentifier, next.Location);
                }
                else if (next.Value.IsLetter() || next.Value == '_')
                {
                    var blob = SqliteTextParsers.SqlBlob(next.Location);
                    if (blob.HasValue)
                    {
                        yield return Result.Value(SqliteToken.Blob, blob.Location, blob.Remainder);
                        next = blob.Remainder.ConsumeChar();
                    }
                    else
                    {
                        var beginIdentifier = next.Location;
                        do
                        {
                            next = next.Remainder.ConsumeChar();
                        }
                        while (next.HasValue && (next.Value.IsLetterOrDigit() || next.Value == '_'));

                        if (TryGetKeyword(beginIdentifier.Until(next.Location), out var keyword))
                            yield return Result.Value(keyword, beginIdentifier, next.Location);
                        else
                            yield return Result.Value(SqliteToken.Identifier, beginIdentifier, next.Location);
                    }
                }
                else if (next.Value == '/' && (!Previous.HasValue || PreRegexTokens.Contains(Previous.Kind)))
                {
                    var sqlComment = SqliteTextParsers.SqlComment(next.Location);
                    if (sqlComment.HasValue)
                    {
                        // don't return comments, assume they're filtered out as it makes parsing more difficult
                        next = sqlComment.Remainder.ConsumeChar();
                    }
                    else
                    {
                        var regex = SqliteTextParsers.RegularExpression(next.Location);
                        if (!regex.HasValue)
                            yield return Result.CastEmpty<Unit, SqliteToken>(regex);

                        yield return Result.Value(SqliteToken.RegularExpression, next.Location, regex.Remainder);
                        next = regex.Remainder.ConsumeChar();
                    }
                }
                else if (next.Value == '-' || next.Value == '/')
                {
                    var sqlComment = SqliteTextParsers.SqlComment(next.Location);
                    if (sqlComment.HasValue)
                    {
                        // don't return comments, assume they're filtered out as it makes parsing more difficult
                        // yield return Result.Value(SqlToken.Comment, sqlComment.Location, sqlComment.Remainder);
                        next = sqlComment.Remainder.ConsumeChar();
                    }
                    else
                    {
                        if (TryGetKeyword(next.Location, out var keyword))
                        {
                            yield return Result.Value(keyword, next.Location, next.Remainder);
                            next = next.Remainder.ConsumeChar();
                        }
                        else
                        {
                            yield return Result.Empty<SqliteToken>(next.Location);
                        }
                    }
                }
                else
                {
                    var compoundOp = SqliteTextParsers.CompoundOperator(next.Location);
                    if (compoundOp.HasValue)
                    {
                        yield return Result.Value(compoundOp.Value, compoundOp.Location, compoundOp.Remainder);
                        next = compoundOp.Remainder.ConsumeChar();
                    }
                    else if (next.Value < SimpleOps.Length && SimpleOps[next.Value] != SqliteToken.None)
                    {
                        yield return Result.Value(SimpleOps[next.Value], next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }
                    else
                    {
                        yield return Result.Empty<SqliteToken>(next.Location);
                        next = next.Remainder.ConsumeChar();
                    }
                }

                next = SkipWhiteSpace(next.Location);
            } while (next.HasValue);
        }

        private static bool TryGetKeyword(TextSpan span, out SqliteToken keyword)
        {
            foreach (var kw in SqlKeywords)
            {
                if (span.EqualsValueIgnoreCase(kw.Text))
                {
                    keyword = kw.Token;
                    return true;
                }
            }

            keyword = SqliteToken.None;
            return false;
        }

        private static readonly SqliteToken[] SimpleOps = new SqliteToken[128];

        private static readonly HashSet<SqliteToken> PreRegexTokens = new HashSet<SqliteToken>
        {
            SqliteToken.And,
            SqliteToken.Or,
            SqliteToken.Not,
            SqliteToken.LParen,
            SqliteToken.LBracket,
            SqliteToken.Comma,
            SqliteToken.Equal,
            SqliteToken.NotEqual,
            SqliteToken.Like,
            SqliteToken.GreaterThan,
            SqliteToken.GreaterThanOrEqual,
            SqliteToken.LessThan,
            SqliteToken.LessThanOrEqual,
            SqliteToken.Is
        };

        private static readonly SqliteKeyword[] SqlKeywords =
        {
            new SqliteKeyword("and", SqliteToken.And),
            new SqliteKeyword("is", SqliteToken.Is),
            new SqliteKeyword("like", SqliteToken.Like),
            new SqliteKeyword("not", SqliteToken.Not),
            new SqliteKeyword("or", SqliteToken.Or),
            new SqliteKeyword("true", SqliteToken.True),
            new SqliteKeyword("false", SqliteToken.False),
            new SqliteKeyword("null", SqliteToken.Null),

            new SqliteKeyword("abort", SqliteToken.Abort),
            new SqliteKeyword("action", SqliteToken.Action),
            new SqliteKeyword("add", SqliteToken.Add),
            new SqliteKeyword("after", SqliteToken.After),
            new SqliteKeyword("all", SqliteToken.All),
            new SqliteKeyword("alter", SqliteToken.Alter),
            new SqliteKeyword("analyze", SqliteToken.Analyze),
            new SqliteKeyword("and", SqliteToken.And),
            new SqliteKeyword("as", SqliteToken.As),
            new SqliteKeyword("asc", SqliteToken.Ascending),
            new SqliteKeyword("attach", SqliteToken.Attach),
            new SqliteKeyword("autoincrement", SqliteToken.Autoincrement),
            new SqliteKeyword("before", SqliteToken.Before),
            new SqliteKeyword("begin", SqliteToken.Before),
            new SqliteKeyword("between", SqliteToken.Between),
            new SqliteKeyword("by", SqliteToken.By),
            new SqliteKeyword("cascade", SqliteToken.Cascade),
            new SqliteKeyword("case", SqliteToken.Case),
            new SqliteKeyword("cast", SqliteToken.Cast),
            new SqliteKeyword("check", SqliteToken.Check),
            new SqliteKeyword("collate", SqliteToken.Collate),
            new SqliteKeyword("column", SqliteToken.Column),
            new SqliteKeyword("commit", SqliteToken.Commit),
            new SqliteKeyword("conflict", SqliteToken.Conflict),
            new SqliteKeyword("constraint", SqliteToken.Constraint),
            new SqliteKeyword("create", SqliteToken.Create),
            new SqliteKeyword("cross", SqliteToken.Cross),
            new SqliteKeyword("current_date", SqliteToken.CurrentDate),
            new SqliteKeyword("current_time", SqliteToken.CurrentTime),
            new SqliteKeyword("current_timestamp", SqliteToken.CurrentTimestamp),
            new SqliteKeyword("database", SqliteToken.Database),
            new SqliteKeyword("default", SqliteToken.Default),
            new SqliteKeyword("deferrable", SqliteToken.Deferrable),
            new SqliteKeyword("deferred", SqliteToken.Deferred),
            new SqliteKeyword("delete", SqliteToken.Delete),
            new SqliteKeyword("desc", SqliteToken.Descending),
            new SqliteKeyword("detach", SqliteToken.Detach),
            new SqliteKeyword("distinct", SqliteToken.Distinct),
            new SqliteKeyword("drop", SqliteToken.Drop),
            new SqliteKeyword("each", SqliteToken.Each),
            new SqliteKeyword("else", SqliteToken.Else),
            new SqliteKeyword("end", SqliteToken.End),
            new SqliteKeyword("escape", SqliteToken.Escape),
            new SqliteKeyword("except", SqliteToken.Except),
            new SqliteKeyword("exclusive", SqliteToken.Exclusive),
            new SqliteKeyword("exists", SqliteToken.Exists),
            new SqliteKeyword("explain", SqliteToken.Explain),
            new SqliteKeyword("fail", SqliteToken.Fail),
            new SqliteKeyword("for", SqliteToken.For),
            new SqliteKeyword("foreign", SqliteToken.Foreign),
            new SqliteKeyword("from", SqliteToken.From),
            new SqliteKeyword("full", SqliteToken.Full),
            new SqliteKeyword("glob", SqliteToken.Glob),
            new SqliteKeyword("group", SqliteToken.Group),
            new SqliteKeyword("having", SqliteToken.Having),
            new SqliteKeyword("if", SqliteToken.If),
            new SqliteKeyword("ignore", SqliteToken.Ignore),
            new SqliteKeyword("immediate", SqliteToken.Immediate),
            new SqliteKeyword("in", SqliteToken.In),
            new SqliteKeyword("index", SqliteToken.Index),
            new SqliteKeyword("indexed", SqliteToken.Indexed),
            new SqliteKeyword("initially", SqliteToken.Initially),
            new SqliteKeyword("inner", SqliteToken.Inner),
            new SqliteKeyword("insert", SqliteToken.Insert),
            new SqliteKeyword("instead", SqliteToken.Instead),
            new SqliteKeyword("intersect", SqliteToken.Intersect),
            new SqliteKeyword("into", SqliteToken.Into),
            new SqliteKeyword("is", SqliteToken.Is),
            new SqliteKeyword("isnull", SqliteToken.IsNull),
            new SqliteKeyword("join", SqliteToken.Join),
            new SqliteKeyword("key", SqliteToken.Key),
            new SqliteKeyword("left", SqliteToken.Left),
            new SqliteKeyword("like", SqliteToken.Like),
            new SqliteKeyword("limit", SqliteToken.Limit),
            new SqliteKeyword("match", SqliteToken.Match),
            new SqliteKeyword("natural", SqliteToken.Natural),
            new SqliteKeyword("no", SqliteToken.No),
            new SqliteKeyword("not", SqliteToken.Not),
            new SqliteKeyword("notnull", SqliteToken.NotNull),
            new SqliteKeyword("null", SqliteToken.Null),
            new SqliteKeyword("of", SqliteToken.Of),
            new SqliteKeyword("offset", SqliteToken.Offset),
            new SqliteKeyword("on", SqliteToken.On),
            new SqliteKeyword("or", SqliteToken.Or),
            new SqliteKeyword("order", SqliteToken.Order),
            new SqliteKeyword("outer", SqliteToken.Outer),
            new SqliteKeyword("plan", SqliteToken.Plan),
            new SqliteKeyword("pragma", SqliteToken.Pragma),
            new SqliteKeyword("primary", SqliteToken.Primary),
            new SqliteKeyword("query", SqliteToken.Query),
            new SqliteKeyword("raise", SqliteToken.Raise),
            new SqliteKeyword("recursive", SqliteToken.Recursive),
            new SqliteKeyword("references", SqliteToken.References),
            new SqliteKeyword("regexp", SqliteToken.Regexp),
            new SqliteKeyword("reindex", SqliteToken.ReIndex),
            new SqliteKeyword("release", SqliteToken.Release),
            new SqliteKeyword("rename", SqliteToken.Release),
            new SqliteKeyword("replace", SqliteToken.Replace),
            new SqliteKeyword("restrict", SqliteToken.Restrict),
            new SqliteKeyword("right", SqliteToken.Right),
            new SqliteKeyword("rollback", SqliteToken.Rollback),
            new SqliteKeyword("row", SqliteToken.Row),
            new SqliteKeyword("savepoint", SqliteToken.Savepoint),
            new SqliteKeyword("select", SqliteToken.Select),
            new SqliteKeyword("set", SqliteToken.Set),
            new SqliteKeyword("table", SqliteToken.Table),
            new SqliteKeyword("temp", SqliteToken.Temporary),
            new SqliteKeyword("temporary", SqliteToken.Temporary),
            new SqliteKeyword("then", SqliteToken.Then),
            new SqliteKeyword("to", SqliteToken.To),
            new SqliteKeyword("transaction", SqliteToken.Transaction),
            new SqliteKeyword("trigger", SqliteToken.Trigger),
            new SqliteKeyword("union", SqliteToken.Union),
            new SqliteKeyword("unique", SqliteToken.Unique),
            new SqliteKeyword("update", SqliteToken.Update),
            new SqliteKeyword("using", SqliteToken.Using),
            new SqliteKeyword("vacuum", SqliteToken.Vacuum),
            new SqliteKeyword("values", SqliteToken.Values),
            new SqliteKeyword("view", SqliteToken.View),
            new SqliteKeyword("virtual", SqliteToken.Virtual),
            new SqliteKeyword("when", SqliteToken.When),
            new SqliteKeyword("where", SqliteToken.Where),
            new SqliteKeyword("with", SqliteToken.With),
            new SqliteKeyword("without", SqliteToken.Without)
        };
    }
}
