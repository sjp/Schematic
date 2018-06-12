using System.Linq;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace SJP.Schematic.Sqlite.Parsing
{
    internal static class SqliteTextParsers
    {
        public static TextParser<string> HexInteger { get; } =
            Span.EqualTo("0x")
                .IgnoreThen(Character.Digit.Or(Character.Matching(ch => (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F'), "a-f"))
                    .Named("hex digit")
                    .AtLeastOnce())
                .Select(chrs => new string(chrs));

        public static TextParser<char> SqlStringContentChar { get; } =
            Span.EqualTo("''").Value('\'').Try().Or(Character.ExceptIn('\'', '\r', '\n'));

        public static TextParser<string> SqlString { get; } =
            Character.EqualTo('\'')
                .IgnoreThen(SqlStringContentChar.Many())
                .Then(s => Character.EqualTo('\'').Value(new string(s)));

        public static TextParser<string> SqlBlob =>
            Span.EqualToIgnoreCase("x")
                .Then(x => SqlString.Select(str => x.ToStringValue() + str));

        public static TextParser<char> SqlInlineCommentChar { get; } =
            Character.ExceptIn('\r', '\n');

        public static TextParser<string> SqlInlineComment { get; } =
            Span.EqualTo("--")
                .Then(prefix => SqlInlineCommentChar.Many().Select(chars => prefix.ToString() + new string(chars)));

        public static TextParser<string> SqlBlockComment { get; } =
            Span.EqualTo("/*")
                .Then(prefix =>
                {
                    char prev = 'x'; // can be anything
                    return Span.WithAll(c =>
                    {
                        var isTerminator = prev == '*' && c == '/';
                        prev = c;
                        return isTerminator;
                    }).Select(c => prefix + c.ToString());
                })
                .Then(prefix => Character.EqualTo('/').Select(c => prefix + c.ToString()));

        public static TextParser<string> SqlComment { get; } =
            SqlInlineComment.Or(SqlBlockComment);

        public static TextParser<char> RegularExpressionContentChar { get; } =
            Span.EqualTo(@"\/").Value('/').Try().Or(Character.Except('/'));

        public static TextParser<Unit> RegularExpression { get; } =
            Character.EqualTo('/')
                .IgnoreThen(RegularExpressionContentChar.Many())
                .IgnoreThen(Character.EqualTo('/'))
                .Value(Unit.Value);

        public static TextParser<TextSpan> Real { get; } =
            Numerics.Integer
                .Then(n => Character.EqualTo('.').IgnoreThen(Numerics.Integer).OptionalOrDefault()
                    .Select(f => f == TextSpan.None ? n : new TextSpan(n.Source, n.Position, n.Length + f.Length + 1)));

        private readonly static TextParser<SqliteToken> LessOrEqual = Span.EqualTo("<=").Value(SqliteToken.LessThanOrEqual);
        private readonly static TextParser<SqliteToken> GreaterOrEqual = Span.EqualTo(">=").Value(SqliteToken.GreaterThanOrEqual);
        private readonly static TextParser<SqliteToken> NotEqual = Span.EqualTo("<>").Value(SqliteToken.NotEqual);
        private readonly static TextParser<SqliteToken> LogicalOr = Span.EqualTo("||").Value(SqliteToken.Or);
        private readonly static TextParser<SqliteToken> LeftShift = Span.EqualTo("<<").Value(SqliteToken.LeftShift);
        private readonly static TextParser<SqliteToken> RightShift = Span.EqualTo(">>").Value(SqliteToken.RightShift);
        private readonly static TextParser<SqliteToken> BangNotEqual = Span.EqualTo("!=").Value(SqliteToken.NotEqual);
        private readonly static TextParser<SqliteToken> DoubleEqual = Span.EqualTo("==").Value(SqliteToken.Equal);
        private readonly static TextParser<SqliteToken> Regexp = Span.EqualTo("REGEXP").Value(SqliteToken.Regexp);
        private readonly static TextParser<SqliteToken> Match = Span.EqualTo("MATCH").Value(SqliteToken.Match);
        private readonly static TextParser<SqliteToken> Glob = Span.EqualTo("GLOB").Value(SqliteToken.Glob);
        private readonly static TextParser<SqliteToken> Like = Span.EqualTo("LIKE").Value(SqliteToken.Like);
        private readonly static TextParser<SqliteToken> And = Span.EqualTo("AND").Value(SqliteToken.And);
        private readonly static TextParser<SqliteToken> Not = Span.EqualTo("NOT").Value(SqliteToken.Not);
        private readonly static TextParser<SqliteToken> In = Span.EqualTo("IN").Value(SqliteToken.In);
        private readonly static TextParser<SqliteToken> Is = Span.EqualTo("IS").Value(SqliteToken.Is);
        private readonly static TextParser<SqliteToken> Or = Span.EqualTo("OR").Value(SqliteToken.Or);

        public static TextParser<SqliteToken> CompoundOperator { get; } =
            GreaterOrEqual
                .Try().Or(LessOrEqual)
                .Try().Or(NotEqual)
                .Try().Or(LogicalOr)
                .Try().Or(LeftShift)
                .Try().Or(RightShift)
                .Try().Or(BangNotEqual)
                .Try().Or(DoubleEqual)
                .Try().Or(Regexp)
                .Try().Or(Match)
                .Try().Or(Glob)
                .Try().Or(Like)
                .Try().Or(And)
                .Try().Or(Not)
                .Try().Or(In)
                .Try().Or(Is)
                .Try().Or(Or);
    }
}
