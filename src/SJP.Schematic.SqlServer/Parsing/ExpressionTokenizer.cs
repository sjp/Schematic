using System.Collections.Generic;
using SJP.Schematic.Core;
using Superpower;
using Superpower.Model;

namespace SJP.Schematic.SqlServer.Parsing
{
    internal class ExpressionTokenizer : Tokenizer<ExpressionToken>
    {
        static ExpressionTokenizer()
        {
            SimpleOps['+'] = ExpressionToken.Plus;
            SimpleOps['-'] = ExpressionToken.Minus;
            SimpleOps['*'] = ExpressionToken.Asterisk;
            SimpleOps['/'] = ExpressionToken.Divide;
            SimpleOps['%'] = ExpressionToken.Percent;
            SimpleOps['&'] = ExpressionToken.BitwiseAnd;
            SimpleOps['|'] = ExpressionToken.BitwiseOr;
            SimpleOps['^'] = ExpressionToken.BitwiseXor;
            SimpleOps['<'] = ExpressionToken.LessThan;
            SimpleOps['>'] = ExpressionToken.GreaterThan;
            SimpleOps['='] = ExpressionToken.Equal;
            SimpleOps[','] = ExpressionToken.Comma;
            SimpleOps['.'] = ExpressionToken.Period;
            SimpleOps['('] = ExpressionToken.LParen;
            SimpleOps[')'] = ExpressionToken.RParen;
            SimpleOps['['] = ExpressionToken.LBracket;
            SimpleOps[']'] = ExpressionToken.RBracket;
            SimpleOps['?'] = ExpressionToken.QuestionMark;
            SimpleOps[';'] = ExpressionToken.Semicolon;
        }

        protected override IEnumerable<Result<ExpressionToken>> Tokenize(TextSpan span)
        {
            var next = SkipWhiteSpace(span);
            if (!next.HasValue)
                yield break;

            do
            {
                if (next.Value.IsDigit())
                {
                    var hex = ExpressionTextParsers.SqlBlob(next.Location);
                    if (hex.HasValue)
                    {
                        next = hex.Remainder.ConsumeChar();
                        yield return Result.Value(ExpressionToken.Blob, hex.Location, hex.Remainder);
                    }
                    else
                    {
                        var real = ExpressionTextParsers.Real(next.Location);
                        if (!real.HasValue)
                            yield return Result.CastEmpty<TextSpan, ExpressionToken>(real);
                        else
                            yield return Result.Value(ExpressionToken.Number, real.Location, real.Remainder);

                        next = real.Remainder.ConsumeChar();
                    }

                    if (next.HasValue && !next.Value.IsPunctuation() && !next.Value.IsWhiteSpace())
                    {
                        yield return Result.Empty<ExpressionToken>(next.Location, new[] { "digit" });
                    }
                }
                else if (next.Value == '$')
                {
                    var money = ExpressionTextParsers.Money(next.Location);
                    if (!money.HasValue)
                        yield return Result.CastEmpty<TextSpan, ExpressionToken>(money);
                    else
                        yield return Result.Value(ExpressionToken.Money, money.Location, money.Remainder);

                    next = money.Remainder.ConsumeChar();
                }
                else if (next.Value == '\'')
                {
                    var str = ExpressionTextParsers.SqlString(next.Location);
                    if (!str.HasValue)
                        yield return Result.CastEmpty<string, ExpressionToken>(str);

                    next = str.Remainder.ConsumeChar();

                    yield return Result.Value(ExpressionToken.String, str.Location, str.Remainder);
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
                        yield return Result.Empty<ExpressionToken>(startOfName, new[] { "built-in identifier name" });
                    }
                    else
                    {
                        yield return Result.Value(ExpressionToken.BuiltInIdentifier, beginIdentifier, next.Location);
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

                    yield return Result.Value(ExpressionToken.Identifier, beginIdentifier, next.Location);
                }
                else if (next.Value.IsLetter() || next.Value == '_')
                {
                    var beginIdentifier = next.Location;
                    do
                    {
                        next = next.Remainder.ConsumeChar();
                    }
                    while (next.HasValue && (next.Value.IsLetterOrDigit() || next.Value == '_'));

                    var identCandidate = beginIdentifier.Until(next.Location);
                    var isNvarcharPrefix = identCandidate.EqualsValueIgnoreCase("N") && next.HasValue && next.Value == '\'';
                    var str = ExpressionTextParsers.SqlString(beginIdentifier);
                    if (str.HasValue)
                    {
                        yield return Result.Value(ExpressionToken.String, str.Location, str.Remainder);
                        next = str.Remainder.ConsumeChar();
                    }
                    else if (TryGetKeyword(beginIdentifier.Until(next.Location), out var keyword))
                    {
                        yield return Result.Value(keyword, beginIdentifier, next.Location);
                    }
                    else
                    {
                        yield return Result.Value(ExpressionToken.Identifier, beginIdentifier, next.Location);
                    }
                }
                else
                {
                    var compoundOp = ExpressionTextParsers.CompoundOperator(next.Location);
                    if (compoundOp.HasValue)
                    {
                        yield return Result.Value(compoundOp.Value, compoundOp.Location, compoundOp.Remainder);
                        next = compoundOp.Remainder.ConsumeChar();
                    }
                    else if (next.Value < SimpleOps.Length && SimpleOps[next.Value] != ExpressionToken.None)
                    {
                        yield return Result.Value(SimpleOps[next.Value], next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }
                    else
                    {
                        yield return Result.Empty<ExpressionToken>(next.Location);
                        next = next.Remainder.ConsumeChar();
                    }
                }

                next = SkipWhiteSpace(next.Location);
            } while (next.HasValue);
        }

        private static bool TryGetKeyword(TextSpan span, out ExpressionToken keyword)
        {
            foreach (var kw in SqlKeywords)
            {
                if (span.EqualsValueIgnoreCase(kw.Text))
                {
                    keyword = kw.Token;
                    return true;
                }
            }

            keyword = ExpressionToken.None;
            return false;
        }

        private static readonly ExpressionToken[] SimpleOps = new ExpressionToken[128];

        private static readonly ExpressionKeyword[] SqlKeywords =
        {
            new ExpressionKeyword("add", ExpressionToken.Add),
            new ExpressionKeyword("all", ExpressionToken.All),
            new ExpressionKeyword("alter", ExpressionToken.Alter),
            new ExpressionKeyword("and", ExpressionToken.And),
            new ExpressionKeyword("any", ExpressionToken.Any),
            new ExpressionKeyword("as", ExpressionToken.As),
            new ExpressionKeyword("asc", ExpressionToken.Ascending),
            new ExpressionKeyword("authorization", ExpressionToken.Authorization),
            new ExpressionKeyword("backup", ExpressionToken.Backup),
            new ExpressionKeyword("begin", ExpressionToken.Begin),
            new ExpressionKeyword("between", ExpressionToken.Between),
            new ExpressionKeyword("break", ExpressionToken.Break),
            new ExpressionKeyword("browse", ExpressionToken.Browse),
            new ExpressionKeyword("bulk", ExpressionToken.Bulk),
            new ExpressionKeyword("by", ExpressionToken.By),
            new ExpressionKeyword("cascade", ExpressionToken.Cascade),
            new ExpressionKeyword("case", ExpressionToken.Case),
            new ExpressionKeyword("check", ExpressionToken.Check),
            new ExpressionKeyword("checkpoint", ExpressionToken.Checkpoint),
            new ExpressionKeyword("close", ExpressionToken.Close),
            new ExpressionKeyword("clustered", ExpressionToken.Clustered),
            new ExpressionKeyword("coalesce", ExpressionToken.Coalesce),
            new ExpressionKeyword("collate", ExpressionToken.Collate),
            new ExpressionKeyword("column", ExpressionToken.Column),
            new ExpressionKeyword("commit", ExpressionToken.Commit),
            new ExpressionKeyword("compute", ExpressionToken.Compute),
            new ExpressionKeyword("constraint", ExpressionToken.Constraint),
            new ExpressionKeyword("contains", ExpressionToken.Contains),
            new ExpressionKeyword("containstable", ExpressionToken.ContainsTable),
            new ExpressionKeyword("continue", ExpressionToken.Continue),
            new ExpressionKeyword("convert", ExpressionToken.Convert),
            new ExpressionKeyword("create", ExpressionToken.Create),
            new ExpressionKeyword("cross", ExpressionToken.Cross),
            new ExpressionKeyword("current", ExpressionToken.Current),
            new ExpressionKeyword("current_date", ExpressionToken.CurrentDate),
            new ExpressionKeyword("current_time", ExpressionToken.CurrentTime),
            new ExpressionKeyword("current_timestamp", ExpressionToken.CurrentTimestamp),
            new ExpressionKeyword("current_user", ExpressionToken.CurrentUser),
            new ExpressionKeyword("cursor", ExpressionToken.Cursor),
            new ExpressionKeyword("database", ExpressionToken.Database),
            new ExpressionKeyword("dbcc", ExpressionToken.Dbcc),
            new ExpressionKeyword("deallocate", ExpressionToken.Deallocate),
            new ExpressionKeyword("declare", ExpressionToken.Declare),
            new ExpressionKeyword("default", ExpressionToken.Default),
            new ExpressionKeyword("delete", ExpressionToken.Delete),
            new ExpressionKeyword("deny", ExpressionToken.Deny),
            new ExpressionKeyword("desc", ExpressionToken.Descending),
            new ExpressionKeyword("disk", ExpressionToken.Disk),
            new ExpressionKeyword("distinct", ExpressionToken.Distinct),
            new ExpressionKeyword("distributed", ExpressionToken.Distributed),
            new ExpressionKeyword("double", ExpressionToken.Double),
            new ExpressionKeyword("drop", ExpressionToken.Drop),
            new ExpressionKeyword("dump", ExpressionToken.Dump),
            new ExpressionKeyword("else", ExpressionToken.Else),
            new ExpressionKeyword("end", ExpressionToken.End),
            new ExpressionKeyword("errlvl", ExpressionToken.ErrorLevel),
            new ExpressionKeyword("escape", ExpressionToken.Escape),
            new ExpressionKeyword("except", ExpressionToken.Except),
            new ExpressionKeyword("exec", ExpressionToken.Exec),
            new ExpressionKeyword("execute", ExpressionToken.Execute),
            new ExpressionKeyword("exists", ExpressionToken.Exists),
            new ExpressionKeyword("exit", ExpressionToken.Exit),
            new ExpressionKeyword("external", ExpressionToken.External),
            new ExpressionKeyword("fetch", ExpressionToken.Fetch),
            new ExpressionKeyword("file", ExpressionToken.File),
            new ExpressionKeyword("fillfactor", ExpressionToken.FillFactor),
            new ExpressionKeyword("for", ExpressionToken.For),
            new ExpressionKeyword("foreign", ExpressionToken.Foreign),
            new ExpressionKeyword("freetext", ExpressionToken.FreeText),
            new ExpressionKeyword("freetexttable", ExpressionToken.FreeTextTable),
            new ExpressionKeyword("from", ExpressionToken.From),
            new ExpressionKeyword("full", ExpressionToken.Full),
            new ExpressionKeyword("function", ExpressionToken.Function),
            new ExpressionKeyword("goto", ExpressionToken.Goto),
            new ExpressionKeyword("grant", ExpressionToken.Grant),
            new ExpressionKeyword("group", ExpressionToken.Group),
            new ExpressionKeyword("having", ExpressionToken.Having),
            new ExpressionKeyword("holdlock", ExpressionToken.HoldLock),
            new ExpressionKeyword("identity", ExpressionToken.Identity),
            new ExpressionKeyword("identity_insert", ExpressionToken.IdentityInsert),
            new ExpressionKeyword("identitycol", ExpressionToken.IdentityCol),
            new ExpressionKeyword("if", ExpressionToken.If),
            new ExpressionKeyword("in", ExpressionToken.In),
            new ExpressionKeyword("index", ExpressionToken.Index),
            new ExpressionKeyword("inner", ExpressionToken.Inner),
            new ExpressionKeyword("insert", ExpressionToken.Insert),
            new ExpressionKeyword("intersect", ExpressionToken.Intersect),
            new ExpressionKeyword("into", ExpressionToken.Into),
            new ExpressionKeyword("is", ExpressionToken.Is),
            new ExpressionKeyword("join", ExpressionToken.Join),
            new ExpressionKeyword("key", ExpressionToken.Key),
            new ExpressionKeyword("kill", ExpressionToken.Kill),
            new ExpressionKeyword("left", ExpressionToken.Left),
            new ExpressionKeyword("like", ExpressionToken.Like),
            new ExpressionKeyword("lineno", ExpressionToken.LineNumber),
            new ExpressionKeyword("load", ExpressionToken.Load),
            new ExpressionKeyword("merge", ExpressionToken.Merge),
            new ExpressionKeyword("national", ExpressionToken.National),
            new ExpressionKeyword("nocheck", ExpressionToken.NoCheck),
            new ExpressionKeyword("nonclustered", ExpressionToken.NonClustered),
            new ExpressionKeyword("not", ExpressionToken.Not),
            new ExpressionKeyword("null", ExpressionToken.Null),
            new ExpressionKeyword("nullif", ExpressionToken.Nullif),
            new ExpressionKeyword("of", ExpressionToken.Of),
            new ExpressionKeyword("off", ExpressionToken.Off),
            new ExpressionKeyword("offsets", ExpressionToken.Offsets),
            new ExpressionKeyword("on", ExpressionToken.On),
            new ExpressionKeyword("open", ExpressionToken.Open),
            new ExpressionKeyword("opendatasource", ExpressionToken.OpenDataSource),
            new ExpressionKeyword("openquery", ExpressionToken.OpenQuery),
            new ExpressionKeyword("openrowset", ExpressionToken.OpenRowSet),
            new ExpressionKeyword("openxml", ExpressionToken.OpenXml),
            new ExpressionKeyword("option", ExpressionToken.Option),
            new ExpressionKeyword("or", ExpressionToken.Or),
            new ExpressionKeyword("order", ExpressionToken.Order),
            new ExpressionKeyword("outer", ExpressionToken.Outer),
            new ExpressionKeyword("over", ExpressionToken.Over),
            new ExpressionKeyword("percent", ExpressionToken.Percent),
            new ExpressionKeyword("pivot", ExpressionToken.Pivot),
            new ExpressionKeyword("plan", ExpressionToken.Plan),
            new ExpressionKeyword("precision", ExpressionToken.Precision),
            new ExpressionKeyword("primary", ExpressionToken.Primary),
            new ExpressionKeyword("print", ExpressionToken.Print),
            new ExpressionKeyword("proc", ExpressionToken.Proc),
            new ExpressionKeyword("procedure", ExpressionToken.Procedure),
            new ExpressionKeyword("public", ExpressionToken.Public),
            new ExpressionKeyword("raiserror", ExpressionToken.Raiserror),
            new ExpressionKeyword("read", ExpressionToken.Read),
            new ExpressionKeyword("readtext", ExpressionToken.ReadText),
            new ExpressionKeyword("reconfigure", ExpressionToken.Reconfigure),
            new ExpressionKeyword("references", ExpressionToken.References),
            new ExpressionKeyword("replication", ExpressionToken.Replication),
            new ExpressionKeyword("restore", ExpressionToken.Restore),
            new ExpressionKeyword("restrict", ExpressionToken.Restrict),
            new ExpressionKeyword("return", ExpressionToken.Return),
            new ExpressionKeyword("revert", ExpressionToken.Revert),
            new ExpressionKeyword("revoke", ExpressionToken.Revoke),
            new ExpressionKeyword("right", ExpressionToken.Right),
            new ExpressionKeyword("rollback", ExpressionToken.Rollback),
            new ExpressionKeyword("rowcount", ExpressionToken.RowCount),
            new ExpressionKeyword("rowguidcol", ExpressionToken.RowGuidCol),
            new ExpressionKeyword("rule", ExpressionToken.Rule),
            new ExpressionKeyword("save", ExpressionToken.Save),
            new ExpressionKeyword("schema", ExpressionToken.Schema),
            new ExpressionKeyword("securityaudit", ExpressionToken.SecurityAudit),
            new ExpressionKeyword("select", ExpressionToken.Select),
            new ExpressionKeyword("semantickeyphrasetable", ExpressionToken.SemanticKeyPhraseTable),
            new ExpressionKeyword("semanticsimilaritydetailstable", ExpressionToken.SemanticSimilarityDetailsTable),
            new ExpressionKeyword("semanticsimilaritytable", ExpressionToken.SemanticSimilarityTable),
            new ExpressionKeyword("session_user", ExpressionToken.SessionUser),
            new ExpressionKeyword("set", ExpressionToken.Set),
            new ExpressionKeyword("setuser", ExpressionToken.SetUser),
            new ExpressionKeyword("shutdown", ExpressionToken.Shutdown),
            new ExpressionKeyword("some", ExpressionToken.Some),
            new ExpressionKeyword("statistics", ExpressionToken.Statistics),
            new ExpressionKeyword("system_user", ExpressionToken.SystemUser),
            new ExpressionKeyword("table", ExpressionToken.Table),
            new ExpressionKeyword("tablesample", ExpressionToken.TableSample),
            new ExpressionKeyword("textsize", ExpressionToken.TextSize),
            new ExpressionKeyword("then", ExpressionToken.Then),
            new ExpressionKeyword("to", ExpressionToken.To),
            new ExpressionKeyword("top", ExpressionToken.Top),
            new ExpressionKeyword("tran", ExpressionToken.Tran),
            new ExpressionKeyword("transaction", ExpressionToken.Transaction),
            new ExpressionKeyword("trigger", ExpressionToken.Trigger),
            new ExpressionKeyword("truncate", ExpressionToken.Truncate),
            new ExpressionKeyword("try_convert", ExpressionToken.TryConvert),
            new ExpressionKeyword("tsequal", ExpressionToken.Tsequal),
            new ExpressionKeyword("union", ExpressionToken.Union),
            new ExpressionKeyword("unique", ExpressionToken.Unique),
            new ExpressionKeyword("unpivot", ExpressionToken.Unpivot),
            new ExpressionKeyword("update", ExpressionToken.Update),
            new ExpressionKeyword("updatetext", ExpressionToken.UpdateText),
            new ExpressionKeyword("use", ExpressionToken.Use),
            new ExpressionKeyword("user", ExpressionToken.User),
            new ExpressionKeyword("values", ExpressionToken.Values),
            new ExpressionKeyword("varying", ExpressionToken.Varying),
            new ExpressionKeyword("view", ExpressionToken.View),
            new ExpressionKeyword("waitfor", ExpressionToken.WaitFor),
            new ExpressionKeyword("when", ExpressionToken.When),
            new ExpressionKeyword("where", ExpressionToken.Where),
            new ExpressionKeyword("while", ExpressionToken.While),
            new ExpressionKeyword("with", ExpressionToken.With),
            new ExpressionKeyword("within", ExpressionToken.Within),
            new ExpressionKeyword("writetext", ExpressionToken.WriteText)
        };
    }
}
