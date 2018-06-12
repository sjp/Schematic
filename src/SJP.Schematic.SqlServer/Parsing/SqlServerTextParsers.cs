using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace SJP.Schematic.SqlServer.Parsing
{
    internal static class SqlServerTextParsers
    {
        public static TextParser<string> SqlBlob { get; } =
            Span.EqualTo("0x")
                .IgnoreThen(Character.Digit.Or(Character.Matching(ch => (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F'), "a-f"))
                    .Named("hex digit")
                    .AtLeastOnce())
                .Select(chrs => new string(chrs));

        public static TextParser<char> SqlStringContentChar { get; } =
            Span.EqualTo("''").Value('\'').Try().Or(Character.ExceptIn('\'', '\r', '\n'));

        public static TextParser<string> SqlString { get; } =
            Character.EqualToIgnoreCase('N').IgnoreThen(Character.EqualTo('\''))
                .Try().Or(Character.EqualTo('\''))
                .IgnoreThen(SqlStringContentChar.Many())
                .Then(s => Character.EqualTo('\'').Value(new string(s)));

        public static TextParser<char> SqlInlineCommentChar { get; } =
            Character.ExceptIn('\r', '\n');

        public static TextParser<string> SqlInlineComment { get; } =
            Span.EqualTo("--")
                .Then(prefix => SqlInlineCommentChar.Many().Select(chars => prefix.ToString() + new string(chars)));

        public static TextParser<string> SqlBlockComment { get; } =
            Span.EqualTo("/*")
                .Then(prefix =>
                {
                    var prev = (char)0; // can be anything, using NUL char to make this clear
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

        public static TextParser<TextSpan> Real { get; } =
            Numerics.Integer
                .Then(n => Character.EqualTo('.').IgnoreThen(Numerics.Integer).OptionalOrDefault()
                    .Select(f => f == TextSpan.None ? n : new TextSpan(n.Source, n.Position, n.Length + f.Length + 1)));

        public static TextParser<TextSpan> Money { get; } =
            Character.EqualTo('$')
                .IgnoreThen(Numerics.Integer
                    .Then(n => Character.EqualTo('.').IgnoreThen(Numerics.Integer).OptionalOrDefault()
                        .Select(f => f == TextSpan.None ? n : new TextSpan(n.Source, n.Position, n.Length + f.Length + 1))));

        private readonly static TextParser<SqlServerToken> LessOrEqual = Span.EqualTo("<=").Value(SqlServerToken.LessThanOrEqual);
        private readonly static TextParser<SqlServerToken> GreaterOrEqual = Span.EqualTo(">=").Value(SqlServerToken.GreaterThanOrEqual);
        private readonly static TextParser<SqlServerToken> NotEqual = Span.EqualTo("<>").Value(SqlServerToken.NotEqual);
        private readonly static TextParser<SqlServerToken> PlusEqual = Span.EqualTo("+=").Value(SqlServerToken.PlusEquals);
        private readonly static TextParser<SqlServerToken> MinusEqual = Span.EqualTo("-=").Value(SqlServerToken.MinusEquals);
        private readonly static TextParser<SqlServerToken> MultiplyEqual = Span.EqualTo("*=").Value(SqlServerToken.MultiplyEquals);
        private readonly static TextParser<SqlServerToken> DivideEqual = Span.EqualTo("/=").Value(SqlServerToken.DivideEquals);
        private readonly static TextParser<SqlServerToken> NonStandardNotEqual = Span.EqualTo("!=").Value(SqlServerToken.NonStandardNotEqual);
        private readonly static TextParser<SqlServerToken> NotLessThan = Span.EqualTo("!<").Value(SqlServerToken.NonStandardNotLessThan);
        private readonly static TextParser<SqlServerToken> NotGreaterThan = Span.EqualTo("!>").Value(SqlServerToken.NonStandardNotGreaterThan);
        private readonly static TextParser<SqlServerToken> BitwiseAndEqual = Span.EqualTo("&=").Value(SqlServerToken.BitwiseAndEqual);
        private readonly static TextParser<SqlServerToken> BitwiseOrEqual = Span.EqualTo("|=").Value(SqlServerToken.BitwiseOrEqual);
        private readonly static TextParser<SqlServerToken> BitwiseXorEqual = Span.EqualTo("^=").Value(SqlServerToken.BitwiseXorEqual);
        private readonly static TextParser<SqlServerToken> Scope = Span.EqualTo("::").Value(SqlServerToken.Scope);

        public static TextParser<SqlServerToken> CompoundOperator { get; } =
            GreaterOrEqual
                .Try().Or(LessOrEqual)
                .Try().Or(NotEqual)
                .Try().Or(PlusEqual)
                .Try().Or(MinusEqual)
                .Try().Or(MultiplyEqual)
                .Try().Or(DivideEqual)
                .Try().Or(NonStandardNotEqual)
                .Try().Or(NotLessThan)
                .Try().Or(NotGreaterThan)
                .Try().Or(BitwiseAndEqual)
                .Try().Or(BitwiseOrEqual)
                .Try().Or(BitwiseXorEqual)
                .Try().Or(Scope);
    }
}
