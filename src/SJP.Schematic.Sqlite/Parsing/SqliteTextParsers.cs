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
                .Then(prefix => Character.AnyChar.Many().Select(chars => prefix.ToString() + new string(chars)))
                .Then(s => Span.EqualTo("*/").Value(s + "*/"));

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

        private static readonly TextParser<SqliteToken> LessOrEqual = Span.EqualTo("<=").Value(SqliteToken.LessThanOrEqual);
        private static readonly TextParser<SqliteToken> GreaterOrEqual = Span.EqualTo(">=").Value(SqliteToken.GreaterThanOrEqual);
        private static readonly TextParser<SqliteToken> NotEqual = Span.EqualTo("<>").Value(SqliteToken.NotEqual);
        private static readonly TextParser<SqliteToken> LogicalOr = Span.EqualTo("||").Value(SqliteToken.Or);
        private static readonly TextParser<SqliteToken> LeftShift = Span.EqualTo("<<").Value(SqliteToken.LeftShift);
        private static readonly TextParser<SqliteToken> RightShift = Span.EqualTo(">>").Value(SqliteToken.RightShift);
        private static readonly TextParser<SqliteToken> BangNotEqual = Span.EqualTo("!=").Value(SqliteToken.NotEqual);
        private static readonly TextParser<SqliteToken> DoubleEqual = Span.EqualTo("==").Value(SqliteToken.Equal);
        private static readonly TextParser<SqliteToken> Regexp = Span.EqualTo("REGEXP").Value(SqliteToken.Regexp);
        private static readonly TextParser<SqliteToken> Match = Span.EqualTo("MATCH").Value(SqliteToken.Match);
        private static readonly TextParser<SqliteToken> Glob = Span.EqualTo("GLOB").Value(SqliteToken.Glob);
        private static readonly TextParser<SqliteToken> Like = Span.EqualTo("LIKE").Value(SqliteToken.Like);
        private static readonly TextParser<SqliteToken> And = Span.EqualTo("AND").Value(SqliteToken.And);
        private static readonly TextParser<SqliteToken> Not = Span.EqualTo("NOT").Value(SqliteToken.Not);
        private static readonly TextParser<SqliteToken> In = Span.EqualTo("IN").Value(SqliteToken.In);
        private static readonly TextParser<SqliteToken> Is = Span.EqualTo("IS").Value(SqliteToken.Is);
        private static readonly TextParser<SqliteToken> Or = Span.EqualTo("OR").Value(SqliteToken.Or);

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
