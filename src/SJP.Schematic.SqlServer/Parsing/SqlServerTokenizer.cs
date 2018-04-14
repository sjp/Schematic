using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;
using Superpower;
using Superpower.Model;

namespace SJP.Schematic.SqlServer.Parsing
{
    internal class SqlServerTokenizer : Tokenizer<SqlServerToken>
    {
        static SqlServerTokenizer()
        {
            SimpleOps['+'] = SqlServerToken.Plus;
            SimpleOps['-'] = SqlServerToken.Minus;
            SimpleOps['*'] = SqlServerToken.Asterisk;
            SimpleOps['/'] = SqlServerToken.Divide;
            SimpleOps['%'] = SqlServerToken.Percent;
            SimpleOps['&'] = SqlServerToken.BitwiseAnd;
            SimpleOps['|'] = SqlServerToken.BitwiseOr;
            SimpleOps['^'] = SqlServerToken.BitwiseXor;
            SimpleOps['<'] = SqlServerToken.LessThan;
            SimpleOps['>'] = SqlServerToken.GreaterThan;
            SimpleOps['='] = SqlServerToken.Equal;
            SimpleOps[','] = SqlServerToken.Comma;
            SimpleOps['.'] = SqlServerToken.Period;
            SimpleOps['('] = SqlServerToken.LParen;
            SimpleOps[')'] = SqlServerToken.RParen;
            SimpleOps['['] = SqlServerToken.LBracket;
            SimpleOps[']'] = SqlServerToken.RBracket;
            SimpleOps['?'] = SqlServerToken.QuestionMark;
            SimpleOps[';'] = SqlServerToken.Semicolon;
        }

        protected override IEnumerable<Result<SqlServerToken>> Tokenize(TextSpan span)
        {
            var next = SkipWhiteSpace(span);
            if (!next.HasValue)
                yield break;

            do
            {
                if (next.Value.IsDigit())
                {
                    var hex = SqlServerTextParsers.SqlBlob(next.Location);
                    if (hex.HasValue)
                    {
                        next = hex.Remainder.ConsumeChar();
                        yield return Result.Value(SqlServerToken.Blob, hex.Location, hex.Remainder);
                    }
                    else
                    {
                        var real = SqlServerTextParsers.Real(next.Location);
                        if (!real.HasValue)
                            yield return Result.CastEmpty<TextSpan, SqlServerToken>(real);
                        else
                            yield return Result.Value(SqlServerToken.Number, real.Location, real.Remainder);

                        next = real.Remainder.ConsumeChar();
                    }

                    if (next.HasValue && !next.Value.IsPunctuation() && !next.Value.IsWhiteSpace())
                    {
                        yield return Result.Empty<SqlServerToken>(next.Location, new[] { "digit" });
                    }
                }
                else if (next.Value == '$')
                {
                    var money = SqlServerTextParsers.Money(next.Location);
                    if (!money.HasValue)
                        yield return Result.CastEmpty<TextSpan, SqlServerToken>(money);
                    else
                        yield return Result.Value(SqlServerToken.Money, money.Location, money.Remainder);

                    next = money.Remainder.ConsumeChar();
                }
                else if (next.Value == '\'')
                {
                    var str = SqlServerTextParsers.SqlString(next.Location);
                    if (!str.HasValue)
                        yield return Result.CastEmpty<string, SqlServerToken>(str);

                    next = str.Remainder.ConsumeChar();

                    yield return Result.Value(SqlServerToken.String, str.Location, str.Remainder);
                }
                else if (next.Value == '@')
                {
                    var beginIdentifier = next.Location;
                    var startOfName = next.Remainder;
                    do
                    {
                        next = next.Remainder.ConsumeChar();
                    }
                    while (next.HasValue && next.Value.IsLetterOrDigit());

                    if (next.Remainder == startOfName)
                    {
                        yield return Result.Empty<SqlServerToken>(startOfName, new[] { "built-in identifier name" });
                    }
                    else
                    {
                        yield return Result.Value(SqlServerToken.BuiltInIdentifier, beginIdentifier, next.Location);
                    }
                }
                else if (next.Value == '"' || next.Value == '[')
                {
                    var endChar = next.Value == '[' ? ']' : next.Value;

                    var beginIdentifier = next.Location;
                    do
                    {
                        next = next.Remainder.ConsumeChar();
                    }
                    while (next.HasValue && (next.Value != endChar));
                    next = next.Remainder.ConsumeChar(); // consume end char

                    yield return Result.Value(SqlServerToken.Identifier, beginIdentifier, next.Location);
                }
                else if (next.Value.IsLetter() || next.Value == '_')
                {
                    var beginIdentifier = next.Location;
                    do
                    {
                        next = next.Remainder.ConsumeChar();
                    }
                    while (next.HasValue && (next.Value.IsLetterOrDigit() || next.Value == '_'));

                    var str = SqlServerTextParsers.SqlString(beginIdentifier);
                    if (str.HasValue)
                    {
                        yield return Result.Value(SqlServerToken.String, str.Location, str.Remainder);
                        next = str.Remainder.ConsumeChar();
                    }
                    else if (TryGetKeyword(beginIdentifier.Until(next.Location), out var keyword))
                    {
                        yield return Result.Value(keyword, beginIdentifier, next.Location);
                    }
                    else
                    {
                        yield return Result.Value(SqlServerToken.Identifier, beginIdentifier, next.Location);
                    }
                }
                else if (next.Value == '-' || next.Value == '/')
                {
                    var sqlComment = SqlServerTextParsers.SqlComment(next.Location);
                    var compoundOp = SqlServerTextParsers.CompoundOperator(next.Location);
                    if (sqlComment.HasValue)
                    {
                        // don't return comments, assume they're filtered out as it makes parsing more difficult
                        next = sqlComment.Remainder.ConsumeChar();
                    }
                    else if (compoundOp.HasValue)
                    {
                        yield return Result.Value(compoundOp.Value, compoundOp.Location, compoundOp.Remainder);
                        next = compoundOp.Remainder.ConsumeChar();
                    }
                    else if (next.Value < SimpleOps.Length && SimpleOps[next.Value] != SqlServerToken.None)
                    {
                        yield return Result.Value(SimpleOps[next.Value], next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }
                    else
                    {
                        yield return Result.Empty<SqlServerToken>(next.Location);
                        next = next.Remainder.ConsumeChar();
                    }
                }
                else
                {
                    var compoundOp = SqlServerTextParsers.CompoundOperator(next.Location);
                    if (compoundOp.HasValue)
                    {
                        yield return Result.Value(compoundOp.Value, compoundOp.Location, compoundOp.Remainder);
                        next = compoundOp.Remainder.ConsumeChar();
                    }
                    else if (next.Value < SimpleOps.Length && SimpleOps[next.Value] != SqlServerToken.None)
                    {
                        yield return Result.Value(SimpleOps[next.Value], next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }
                    else
                    {
                        yield return Result.Empty<SqlServerToken>(next.Location);
                        next = next.Remainder.ConsumeChar();
                    }
                }

                next = SkipWhiteSpace(next.Location);
            } while (next.HasValue);
        }

        private static bool TryGetKeyword(TextSpan span, out SqlServerToken keyword)
        {
            foreach (var kw in SqlKeywords)
            {
                if (span.EqualsValueIgnoreCase(kw.Text))
                {
                    keyword = kw.Token;
                    return true;
                }
            }

            keyword = SqlServerToken.None;
            return false;
        }

        private static readonly SqlServerToken[] SimpleOps = new SqlServerToken[128];

        private static readonly SqlServerKeyword[] SqlKeywords =
        {
            new SqlServerKeyword("add", SqlServerToken.Add),
            new SqlServerKeyword("all", SqlServerToken.All),
            new SqlServerKeyword("alter", SqlServerToken.Alter),
            new SqlServerKeyword("and", SqlServerToken.And),
            new SqlServerKeyword("any", SqlServerToken.Any),
            new SqlServerKeyword("as", SqlServerToken.As),
            new SqlServerKeyword("asc", SqlServerToken.Ascending),
            new SqlServerKeyword("authorization", SqlServerToken.Authorization),
            new SqlServerKeyword("backup", SqlServerToken.Backup),
            new SqlServerKeyword("begin", SqlServerToken.Begin),
            new SqlServerKeyword("between", SqlServerToken.Between),
            new SqlServerKeyword("break", SqlServerToken.Break),
            new SqlServerKeyword("browse", SqlServerToken.Browse),
            new SqlServerKeyword("bulk", SqlServerToken.Bulk),
            new SqlServerKeyword("by", SqlServerToken.By),
            new SqlServerKeyword("cascade", SqlServerToken.Cascade),
            new SqlServerKeyword("case", SqlServerToken.Case),
            new SqlServerKeyword("check", SqlServerToken.Check),
            new SqlServerKeyword("checkpoint", SqlServerToken.Checkpoint),
            new SqlServerKeyword("close", SqlServerToken.Close),
            new SqlServerKeyword("clustered", SqlServerToken.Clustered),
            new SqlServerKeyword("coalesce", SqlServerToken.Coalesce),
            new SqlServerKeyword("collate", SqlServerToken.Collate),
            new SqlServerKeyword("column", SqlServerToken.Column),
            new SqlServerKeyword("commit", SqlServerToken.Commit),
            new SqlServerKeyword("compute", SqlServerToken.Compute),
            new SqlServerKeyword("constraint", SqlServerToken.Constraint),
            new SqlServerKeyword("contains", SqlServerToken.Contains),
            new SqlServerKeyword("containstable", SqlServerToken.ContainsTable),
            new SqlServerKeyword("continue", SqlServerToken.Continue),
            new SqlServerKeyword("convert", SqlServerToken.Convert),
            new SqlServerKeyword("create", SqlServerToken.Create),
            new SqlServerKeyword("cross", SqlServerToken.Cross),
            new SqlServerKeyword("current", SqlServerToken.Current),
            new SqlServerKeyword("current_date", SqlServerToken.CurrentDate),
            new SqlServerKeyword("current_time", SqlServerToken.CurrentTime),
            new SqlServerKeyword("current_timestamp", SqlServerToken.CurrentTimestamp),
            new SqlServerKeyword("current_user", SqlServerToken.CurrentUser),
            new SqlServerKeyword("cursor", SqlServerToken.Cursor),
            new SqlServerKeyword("database", SqlServerToken.Database),
            new SqlServerKeyword("dbcc", SqlServerToken.Dbcc),
            new SqlServerKeyword("deallocate", SqlServerToken.Deallocate),
            new SqlServerKeyword("declare", SqlServerToken.Declare),
            new SqlServerKeyword("default", SqlServerToken.Default),
            new SqlServerKeyword("delete", SqlServerToken.Delete),
            new SqlServerKeyword("deny", SqlServerToken.Deny),
            new SqlServerKeyword("desc", SqlServerToken.Descending),
            new SqlServerKeyword("disk", SqlServerToken.Disk),
            new SqlServerKeyword("distinct", SqlServerToken.Distinct),
            new SqlServerKeyword("distributed", SqlServerToken.Distributed),
            new SqlServerKeyword("double", SqlServerToken.Double),
            new SqlServerKeyword("drop", SqlServerToken.Drop),
            new SqlServerKeyword("dump", SqlServerToken.Dump),
            new SqlServerKeyword("else", SqlServerToken.Else),
            new SqlServerKeyword("end", SqlServerToken.End),
            new SqlServerKeyword("errlvl", SqlServerToken.ErrorLevel),
            new SqlServerKeyword("escape", SqlServerToken.Escape),
            new SqlServerKeyword("except", SqlServerToken.Except),
            new SqlServerKeyword("exec", SqlServerToken.Exec),
            new SqlServerKeyword("execute", SqlServerToken.Execute),
            new SqlServerKeyword("exists", SqlServerToken.Exists),
            new SqlServerKeyword("exit", SqlServerToken.Exit),
            new SqlServerKeyword("external", SqlServerToken.External),
            new SqlServerKeyword("fetch", SqlServerToken.Fetch),
            new SqlServerKeyword("file", SqlServerToken.File),
            new SqlServerKeyword("fillfactor", SqlServerToken.FillFactor),
            new SqlServerKeyword("for", SqlServerToken.For),
            new SqlServerKeyword("foreign", SqlServerToken.Foreign),
            new SqlServerKeyword("freetext", SqlServerToken.FreeText),
            new SqlServerKeyword("freetexttable", SqlServerToken.FreeTextTable),
            new SqlServerKeyword("from", SqlServerToken.From),
            new SqlServerKeyword("full", SqlServerToken.Full),
            new SqlServerKeyword("function", SqlServerToken.Function),
            new SqlServerKeyword("goto", SqlServerToken.Goto),
            new SqlServerKeyword("grant", SqlServerToken.Grant),
            new SqlServerKeyword("group", SqlServerToken.Group),
            new SqlServerKeyword("having", SqlServerToken.Having),
            new SqlServerKeyword("holdlock", SqlServerToken.HoldLock),
            new SqlServerKeyword("identity", SqlServerToken.Identity),
            new SqlServerKeyword("identity_insert", SqlServerToken.IdentityInsert),
            new SqlServerKeyword("identitycol", SqlServerToken.IdentityCol),
            new SqlServerKeyword("if", SqlServerToken.If),
            new SqlServerKeyword("in", SqlServerToken.In),
            new SqlServerKeyword("index", SqlServerToken.Index),
            new SqlServerKeyword("inner", SqlServerToken.Inner),
            new SqlServerKeyword("insert", SqlServerToken.Insert),
            new SqlServerKeyword("intersect", SqlServerToken.Intersect),
            new SqlServerKeyword("into", SqlServerToken.Into),
            new SqlServerKeyword("is", SqlServerToken.Is),
            new SqlServerKeyword("join", SqlServerToken.Join),
            new SqlServerKeyword("key", SqlServerToken.Key),
            new SqlServerKeyword("kill", SqlServerToken.Kill),
            new SqlServerKeyword("left", SqlServerToken.Left),
            new SqlServerKeyword("like", SqlServerToken.Like),
            new SqlServerKeyword("lineno", SqlServerToken.LineNumber),
            new SqlServerKeyword("load", SqlServerToken.Load),
            new SqlServerKeyword("merge", SqlServerToken.Merge),
            new SqlServerKeyword("national", SqlServerToken.National),
            new SqlServerKeyword("nocheck", SqlServerToken.NoCheck),
            new SqlServerKeyword("nonclustered", SqlServerToken.NonClustered),
            new SqlServerKeyword("not", SqlServerToken.Not),
            new SqlServerKeyword("null", SqlServerToken.Null),
            new SqlServerKeyword("nullif", SqlServerToken.Nullif),
            new SqlServerKeyword("of", SqlServerToken.Of),
            new SqlServerKeyword("off", SqlServerToken.Off),
            new SqlServerKeyword("offsets", SqlServerToken.Offsets),
            new SqlServerKeyword("on", SqlServerToken.On),
            new SqlServerKeyword("open", SqlServerToken.Open),
            new SqlServerKeyword("opendatasource", SqlServerToken.OpenDataSource),
            new SqlServerKeyword("openquery", SqlServerToken.OpenQuery),
            new SqlServerKeyword("openrowset", SqlServerToken.OpenRowSet),
            new SqlServerKeyword("openxml", SqlServerToken.OpenXml),
            new SqlServerKeyword("option", SqlServerToken.Option),
            new SqlServerKeyword("or", SqlServerToken.Or),
            new SqlServerKeyword("order", SqlServerToken.Order),
            new SqlServerKeyword("outer", SqlServerToken.Outer),
            new SqlServerKeyword("over", SqlServerToken.Over),
            new SqlServerKeyword("percent", SqlServerToken.Percent),
            new SqlServerKeyword("pivot", SqlServerToken.Pivot),
            new SqlServerKeyword("plan", SqlServerToken.Plan),
            new SqlServerKeyword("precision", SqlServerToken.Precision),
            new SqlServerKeyword("primary", SqlServerToken.Primary),
            new SqlServerKeyword("print", SqlServerToken.Print),
            new SqlServerKeyword("proc", SqlServerToken.Proc),
            new SqlServerKeyword("procedure", SqlServerToken.Procedure),
            new SqlServerKeyword("public", SqlServerToken.Public),
            new SqlServerKeyword("raiserror", SqlServerToken.Raiserror),
            new SqlServerKeyword("read", SqlServerToken.Read),
            new SqlServerKeyword("readtext", SqlServerToken.ReadText),
            new SqlServerKeyword("reconfigure", SqlServerToken.Reconfigure),
            new SqlServerKeyword("references", SqlServerToken.References),
            new SqlServerKeyword("replication", SqlServerToken.Replication),
            new SqlServerKeyword("restore", SqlServerToken.Restore),
            new SqlServerKeyword("restrict", SqlServerToken.Restrict),
            new SqlServerKeyword("return", SqlServerToken.Return),
            new SqlServerKeyword("revert", SqlServerToken.Revert),
            new SqlServerKeyword("revoke", SqlServerToken.Revoke),
            new SqlServerKeyword("right", SqlServerToken.Right),
            new SqlServerKeyword("rollback", SqlServerToken.Rollback),
            new SqlServerKeyword("rowcount", SqlServerToken.RowCount),
            new SqlServerKeyword("rowguidcol", SqlServerToken.RowGuidCol),
            new SqlServerKeyword("rule", SqlServerToken.Rule),
            new SqlServerKeyword("save", SqlServerToken.Save),
            new SqlServerKeyword("schema", SqlServerToken.Schema),
            new SqlServerKeyword("securityaudit", SqlServerToken.SecurityAudit),
            new SqlServerKeyword("select", SqlServerToken.Select),
            new SqlServerKeyword("semantickeyphrasetable", SqlServerToken.SemanticKeyPhraseTable),
            new SqlServerKeyword("semanticsimilaritydetailstable", SqlServerToken.SemanticSimilarityDetailsTable),
            new SqlServerKeyword("semanticsimilaritytable", SqlServerToken.SemanticSimilarityTable),
            new SqlServerKeyword("session_user", SqlServerToken.SessionUser),
            new SqlServerKeyword("set", SqlServerToken.Set),
            new SqlServerKeyword("setuser", SqlServerToken.SetUser),
            new SqlServerKeyword("shutdown", SqlServerToken.Shutdown),
            new SqlServerKeyword("some", SqlServerToken.Some),
            new SqlServerKeyword("statistics", SqlServerToken.Statistics),
            new SqlServerKeyword("system_user", SqlServerToken.SystemUser),
            new SqlServerKeyword("table", SqlServerToken.Table),
            new SqlServerKeyword("tablesample", SqlServerToken.TableSample),
            new SqlServerKeyword("textsize", SqlServerToken.TextSize),
            new SqlServerKeyword("then", SqlServerToken.Then),
            new SqlServerKeyword("to", SqlServerToken.To),
            new SqlServerKeyword("top", SqlServerToken.Top),
            new SqlServerKeyword("tran", SqlServerToken.Tran),
            new SqlServerKeyword("transaction", SqlServerToken.Transaction),
            new SqlServerKeyword("trigger", SqlServerToken.Trigger),
            new SqlServerKeyword("truncate", SqlServerToken.Truncate),
            new SqlServerKeyword("try_convert", SqlServerToken.TryConvert),
            new SqlServerKeyword("tsequal", SqlServerToken.Tsequal),
            new SqlServerKeyword("union", SqlServerToken.Union),
            new SqlServerKeyword("unique", SqlServerToken.Unique),
            new SqlServerKeyword("unpivot", SqlServerToken.Unpivot),
            new SqlServerKeyword("update", SqlServerToken.Update),
            new SqlServerKeyword("updatetext", SqlServerToken.UpdateText),
            new SqlServerKeyword("use", SqlServerToken.Use),
            new SqlServerKeyword("user", SqlServerToken.User),
            new SqlServerKeyword("values", SqlServerToken.Values),
            new SqlServerKeyword("varying", SqlServerToken.Varying),
            new SqlServerKeyword("view", SqlServerToken.View),
            new SqlServerKeyword("waitfor", SqlServerToken.WaitFor),
            new SqlServerKeyword("when", SqlServerToken.When),
            new SqlServerKeyword("where", SqlServerToken.Where),
            new SqlServerKeyword("while", SqlServerToken.While),
            new SqlServerKeyword("with", SqlServerToken.With),
            new SqlServerKeyword("within", SqlServerToken.Within),
            new SqlServerKeyword("writetext", SqlServerToken.WriteText)
        };
    }
}
