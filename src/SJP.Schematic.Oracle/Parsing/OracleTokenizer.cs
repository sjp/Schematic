using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core.Extensions;
using Superpower;
using Superpower.Model;

namespace SJP.Schematic.Oracle.Parsing
{
    internal sealed class OracleTokenizer : Tokenizer<OracleToken>
    {
        static OracleTokenizer()
        {
            SimpleOps['+'] = OracleToken.Plus;
            SimpleOps['-'] = OracleToken.Minus;
            SimpleOps['*'] = OracleToken.Asterisk;
            SimpleOps['/'] = OracleToken.Divide;
            SimpleOps['%'] = OracleToken.Percent;
            SimpleOps['&'] = OracleToken.BitwiseAnd;
            SimpleOps['|'] = OracleToken.BitwiseOr;
            SimpleOps['^'] = OracleToken.BitwiseXor;
            SimpleOps['<'] = OracleToken.LessThan;
            SimpleOps['>'] = OracleToken.GreaterThan;
            SimpleOps['='] = OracleToken.Equal;
            SimpleOps[','] = OracleToken.Comma;
            SimpleOps['.'] = OracleToken.Period;
            SimpleOps['('] = OracleToken.LParen;
            SimpleOps[')'] = OracleToken.RParen;
            SimpleOps['['] = OracleToken.LBracket;
            SimpleOps[']'] = OracleToken.RBracket;
            SimpleOps['?'] = OracleToken.QuestionMark;
            SimpleOps[';'] = OracleToken.Semicolon;
        }

        protected override IEnumerable<Result<OracleToken>> Tokenize(TextSpan span)
        {
            var next = SkipWhiteSpace(span);
            if (!next.HasValue)
                yield break;

            do
            {
                if (next.Value.IsDigit())
                {
                    var hex = OracleTextParsers.SqlBlob(next.Location);
                    if (hex.HasValue)
                    {
                        next = hex.Remainder.ConsumeChar();
                        yield return Result.Value(OracleToken.Blob, hex.Location, hex.Remainder);
                    }
                    else
                    {
                        var real = OracleTextParsers.Real(next.Location);
                        if (!real.HasValue)
                        {
                            // handle case where this could be a for loop
                            // e.g. 1..10
                            var integer = OracleTextParsers.Integer(next.Location);
                            if (!integer.HasValue)
                                yield return Result.CastEmpty<TextSpan, OracleToken>(integer);
                            else
                                yield return Result.Value(OracleToken.Number, integer.Location, integer.Remainder);
                        }
                        else
                        {
                            yield return Result.Value(OracleToken.Number, real.Location, real.Remainder);
                            next = real.Remainder.ConsumeChar();
                            next = SkipWhiteSpace(next.Location);
                            continue;
                        }

                        next = real.Remainder.ConsumeChar();
                    }

                    if (next.HasValue && !next.Value.IsPunctuation() && !next.Value.IsWhiteSpace())
                    {
                        yield return Result.Empty<OracleToken>(next.Location, new[] { "digit" });
                    }
                }
                else if (next.Value == '\'')
                {
                    var str = OracleTextParsers.OracleString(next.Location);
                    if (!str.HasValue)
                        yield return Result.CastEmpty<string, OracleToken>(str);

                    next = str.Remainder.ConsumeChar();

                    yield return Result.Value(OracleToken.String, str.Location, str.Remainder);
                }
                else if (next.Value == '"')
                {
                    var endChar = next.Value;

                    var beginIdentifier = next.Location;
                    do
                    {
                        next = next.Remainder.ConsumeChar();
                    }
                    while (next.HasValue && (next.Value != endChar));
                    next = next.Remainder.ConsumeChar(); // consume end char

                    yield return Result.Value(OracleToken.Identifier, beginIdentifier, next.Location);
                }
                else if (next.Value.IsLetter() || AdditionalIdentifierChars.Contains(next.Value))
                {
                    var beginIdentifier = next.Location;
                    do
                    {
                        next = next.Remainder.ConsumeChar();
                    }
                    while (next.HasValue && (next.Value.IsLetterOrDigit() || AdditionalIdentifierChars.Contains(next.Value)));

                    var str = OracleTextParsers.OracleString(beginIdentifier);
                    if (str.HasValue)
                    {
                        yield return Result.Value(OracleToken.String, str.Location, str.Remainder);
                        next = str.Remainder.ConsumeChar();
                    }
                    else if (TryGetKeyword(beginIdentifier.Until(next.Location), out var keyword))
                    {
                        yield return Result.Value(keyword, beginIdentifier, next.Location);
                    }
                    else
                    {
                        yield return Result.Value(OracleToken.Identifier, beginIdentifier, next.Location);
                    }
                }
                else if (next.Value == '-' || next.Value == '/' || next.Value == '|')
                {
                    var sqlComment = OracleTextParsers.SqlComment(next.Location);
                    var compoundOp = OracleTextParsers.CompoundOperator(next.Location);
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
                    else if (next.Value < SimpleOps.Length && SimpleOps[next.Value] != OracleToken.None)
                    {
                        yield return Result.Value(SimpleOps[next.Value], next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }
                    else
                    {
                        yield return Result.Empty<OracleToken>(next.Location);
                        next = next.Remainder.ConsumeChar();
                    }
                }
                else
                {
                    var compoundOp = OracleTextParsers.CompoundOperator(next.Location);
                    if (compoundOp.HasValue)
                    {
                        yield return Result.Value(compoundOp.Value, compoundOp.Location, compoundOp.Remainder);
                        next = compoundOp.Remainder.ConsumeChar();
                    }
                    else if (next.Value < SimpleOps.Length && SimpleOps[next.Value] != OracleToken.None)
                    {
                        yield return Result.Value(SimpleOps[next.Value], next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }
                    else
                    {
                        yield return Result.Empty<OracleToken>(next.Location);
                        next = next.Remainder.ConsumeChar();
                    }
                }

                next = SkipWhiteSpace(next.Location);
            } while (next.HasValue);
        }

        private static bool TryGetKeyword(TextSpan span, out OracleToken keyword)
        {
            foreach (var kw in SqlKeywords)
            {
                if (span.EqualsValueIgnoreCase(kw.Text))
                {
                    keyword = kw.Token;
                    return true;
                }
            }

            keyword = OracleToken.None;
            return false;
        }

        private static readonly char[] AdditionalIdentifierChars = new[] { '$', '#', '_' };

        private static readonly OracleToken[] SimpleOps = new OracleToken[128];

        private static readonly OracleKeyword[] SqlKeywords =
        {
            new OracleKeyword("add", OracleToken.Add),
            new OracleKeyword("all", OracleToken.All),
            new OracleKeyword("alter", OracleToken.Alter),
            new OracleKeyword("and", OracleToken.And),
            new OracleKeyword("any", OracleToken.Any),
            new OracleKeyword("as", OracleToken.As),
            new OracleKeyword("asc", OracleToken.Ascending),
            new OracleKeyword("authorization", OracleToken.Authorization),
            new OracleKeyword("backup", OracleToken.Backup),
            new OracleKeyword("begin", OracleToken.Begin),
            new OracleKeyword("between", OracleToken.Between),
            new OracleKeyword("body", OracleToken.Body),
            new OracleKeyword("break", OracleToken.Break),
            new OracleKeyword("browse", OracleToken.Browse),
            new OracleKeyword("bulk", OracleToken.Bulk),
            new OracleKeyword("by", OracleToken.By),
            new OracleKeyword("cascade", OracleToken.Cascade),
            new OracleKeyword("case", OracleToken.Case),
            new OracleKeyword("check", OracleToken.Check),
            new OracleKeyword("checkpoint", OracleToken.Checkpoint),
            new OracleKeyword("close", OracleToken.Close),
            new OracleKeyword("clustered", OracleToken.Clustered),
            new OracleKeyword("coalesce", OracleToken.Coalesce),
            new OracleKeyword("collate", OracleToken.Collate),
            new OracleKeyword("column", OracleToken.Column),
            new OracleKeyword("commit", OracleToken.Commit),
            new OracleKeyword("compute", OracleToken.Compute),
            new OracleKeyword("constraint", OracleToken.Constraint),
            new OracleKeyword("contains", OracleToken.Contains),
            new OracleKeyword("containstable", OracleToken.ContainsTable),
            new OracleKeyword("continue", OracleToken.Continue),
            new OracleKeyword("convert", OracleToken.Convert),
            new OracleKeyword("create", OracleToken.Create),
            new OracleKeyword("cross", OracleToken.Cross),
            new OracleKeyword("current", OracleToken.Current),
            new OracleKeyword("current_date", OracleToken.CurrentDate),
            new OracleKeyword("current_time", OracleToken.CurrentTime),
            new OracleKeyword("current_timestamp", OracleToken.CurrentTimestamp),
            new OracleKeyword("current_user", OracleToken.CurrentUser),
            new OracleKeyword("cursor", OracleToken.Cursor),
            new OracleKeyword("database", OracleToken.Database),
            new OracleKeyword("dbcc", OracleToken.Dbcc),
            new OracleKeyword("deallocate", OracleToken.Deallocate),
            new OracleKeyword("declare", OracleToken.Declare),
            new OracleKeyword("default", OracleToken.Default),
            new OracleKeyword("delete", OracleToken.Delete),
            new OracleKeyword("deny", OracleToken.Deny),
            new OracleKeyword("desc", OracleToken.Descending),
            new OracleKeyword("disk", OracleToken.Disk),
            new OracleKeyword("distinct", OracleToken.Distinct),
            new OracleKeyword("distributed", OracleToken.Distributed),
            new OracleKeyword("double", OracleToken.Double),
            new OracleKeyword("drop", OracleToken.Drop),
            new OracleKeyword("dump", OracleToken.Dump),
            new OracleKeyword("else", OracleToken.Else),
            new OracleKeyword("end", OracleToken.End),
            new OracleKeyword("errlvl", OracleToken.ErrorLevel),
            new OracleKeyword("escape", OracleToken.Escape),
            new OracleKeyword("except", OracleToken.Except),
            new OracleKeyword("exec", OracleToken.Exec),
            new OracleKeyword("execute", OracleToken.Execute),
            new OracleKeyword("exists", OracleToken.Exists),
            new OracleKeyword("exit", OracleToken.Exit),
            new OracleKeyword("external", OracleToken.External),
            new OracleKeyword("fetch", OracleToken.Fetch),
            new OracleKeyword("file", OracleToken.File),
            new OracleKeyword("fillfactor", OracleToken.FillFactor),
            new OracleKeyword("for", OracleToken.For),
            new OracleKeyword("foreign", OracleToken.Foreign),
            new OracleKeyword("freetext", OracleToken.FreeText),
            new OracleKeyword("freetexttable", OracleToken.FreeTextTable),
            new OracleKeyword("from", OracleToken.From),
            new OracleKeyword("full", OracleToken.Full),
            new OracleKeyword("function", OracleToken.Function),
            new OracleKeyword("goto", OracleToken.Goto),
            new OracleKeyword("grant", OracleToken.Grant),
            new OracleKeyword("group", OracleToken.Group),
            new OracleKeyword("having", OracleToken.Having),
            new OracleKeyword("holdlock", OracleToken.HoldLock),
            new OracleKeyword("identity", OracleToken.Identity),
            new OracleKeyword("identity_insert", OracleToken.IdentityInsert),
            new OracleKeyword("identitycol", OracleToken.IdentityCol),
            new OracleKeyword("if", OracleToken.If),
            new OracleKeyword("in", OracleToken.In),
            new OracleKeyword("index", OracleToken.Index),
            new OracleKeyword("inner", OracleToken.Inner),
            new OracleKeyword("insert", OracleToken.Insert),
            new OracleKeyword("intersect", OracleToken.Intersect),
            new OracleKeyword("into", OracleToken.Into),
            new OracleKeyword("is", OracleToken.Is),
            new OracleKeyword("join", OracleToken.Join),
            new OracleKeyword("key", OracleToken.Key),
            new OracleKeyword("kill", OracleToken.Kill),
            new OracleKeyword("left", OracleToken.Left),
            new OracleKeyword("like", OracleToken.Like),
            new OracleKeyword("lineno", OracleToken.LineNumber),
            new OracleKeyword("load", OracleToken.Load),
            new OracleKeyword("merge", OracleToken.Merge),
            new OracleKeyword("national", OracleToken.National),
            new OracleKeyword("nocheck", OracleToken.NoCheck),
            new OracleKeyword("nonclustered", OracleToken.NonClustered),
            new OracleKeyword("not", OracleToken.Not),
            new OracleKeyword("null", OracleToken.Null),
            new OracleKeyword("nullif", OracleToken.Nullif),
            new OracleKeyword("of", OracleToken.Of),
            new OracleKeyword("off", OracleToken.Off),
            new OracleKeyword("offsets", OracleToken.Offsets),
            new OracleKeyword("on", OracleToken.On),
            new OracleKeyword("open", OracleToken.Open),
            new OracleKeyword("opendatasource", OracleToken.OpenDataSource),
            new OracleKeyword("openquery", OracleToken.OpenQuery),
            new OracleKeyword("openrowset", OracleToken.OpenRowSet),
            new OracleKeyword("openxml", OracleToken.OpenXml),
            new OracleKeyword("option", OracleToken.Option),
            new OracleKeyword("or", OracleToken.Or),
            new OracleKeyword("order", OracleToken.Order),
            new OracleKeyword("outer", OracleToken.Outer),
            new OracleKeyword("over", OracleToken.Over),
            new OracleKeyword("package", OracleToken.Package),
            new OracleKeyword("percent", OracleToken.Percent),
            new OracleKeyword("pivot", OracleToken.Pivot),
            new OracleKeyword("plan", OracleToken.Plan),
            new OracleKeyword("precision", OracleToken.Precision),
            new OracleKeyword("primary", OracleToken.Primary),
            new OracleKeyword("print", OracleToken.Print),
            new OracleKeyword("proc", OracleToken.Proc),
            new OracleKeyword("procedure", OracleToken.Procedure),
            new OracleKeyword("public", OracleToken.Public),
            new OracleKeyword("raiserror", OracleToken.Raiserror),
            new OracleKeyword("read", OracleToken.Read),
            new OracleKeyword("readtext", OracleToken.ReadText),
            new OracleKeyword("reconfigure", OracleToken.Reconfigure),
            new OracleKeyword("references", OracleToken.References),
            new OracleKeyword("replace", OracleToken.Replace),
            new OracleKeyword("replication", OracleToken.Replication),
            new OracleKeyword("restore", OracleToken.Restore),
            new OracleKeyword("restrict", OracleToken.Restrict),
            new OracleKeyword("return", OracleToken.Return),
            new OracleKeyword("revert", OracleToken.Revert),
            new OracleKeyword("revoke", OracleToken.Revoke),
            new OracleKeyword("right", OracleToken.Right),
            new OracleKeyword("rollback", OracleToken.Rollback),
            new OracleKeyword("rowcount", OracleToken.RowCount),
            new OracleKeyword("rowguidcol", OracleToken.RowGuidCol),
            new OracleKeyword("rule", OracleToken.Rule),
            new OracleKeyword("save", OracleToken.Save),
            new OracleKeyword("schema", OracleToken.Schema),
            new OracleKeyword("securityaudit", OracleToken.SecurityAudit),
            new OracleKeyword("select", OracleToken.Select),
            new OracleKeyword("semantickeyphrasetable", OracleToken.SemanticKeyPhraseTable),
            new OracleKeyword("semanticsimilaritydetailstable", OracleToken.SemanticSimilarityDetailsTable),
            new OracleKeyword("semanticsimilaritytable", OracleToken.SemanticSimilarityTable),
            new OracleKeyword("session_user", OracleToken.SessionUser),
            new OracleKeyword("set", OracleToken.Set),
            new OracleKeyword("setuser", OracleToken.SetUser),
            new OracleKeyword("shutdown", OracleToken.Shutdown),
            new OracleKeyword("some", OracleToken.Some),
            new OracleKeyword("statistics", OracleToken.Statistics),
            new OracleKeyword("system_user", OracleToken.SystemUser),
            new OracleKeyword("table", OracleToken.Table),
            new OracleKeyword("tablesample", OracleToken.TableSample),
            new OracleKeyword("textsize", OracleToken.TextSize),
            new OracleKeyword("then", OracleToken.Then),
            new OracleKeyword("to", OracleToken.To),
            new OracleKeyword("top", OracleToken.Top),
            new OracleKeyword("tran", OracleToken.Tran),
            new OracleKeyword("transaction", OracleToken.Transaction),
            new OracleKeyword("trigger", OracleToken.Trigger),
            new OracleKeyword("truncate", OracleToken.Truncate),
            new OracleKeyword("try_convert", OracleToken.TryConvert),
            new OracleKeyword("tsequal", OracleToken.Tsequal),
            new OracleKeyword("union", OracleToken.Union),
            new OracleKeyword("unique", OracleToken.Unique),
            new OracleKeyword("unpivot", OracleToken.Unpivot),
            new OracleKeyword("update", OracleToken.Update),
            new OracleKeyword("updatetext", OracleToken.UpdateText),
            new OracleKeyword("use", OracleToken.Use),
            new OracleKeyword("user", OracleToken.User),
            new OracleKeyword("values", OracleToken.Values),
            new OracleKeyword("varying", OracleToken.Varying),
            new OracleKeyword("view", OracleToken.View),
            new OracleKeyword("waitfor", OracleToken.WaitFor),
            new OracleKeyword("when", OracleToken.When),
            new OracleKeyword("where", OracleToken.Where),
            new OracleKeyword("while", OracleToken.While),
            new OracleKeyword("with", OracleToken.With),
            new OracleKeyword("within", OracleToken.Within),
            new OracleKeyword("writetext", OracleToken.WriteText)
        };
    }
}
