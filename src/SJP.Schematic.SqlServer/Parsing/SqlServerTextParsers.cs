using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace SJP.Schematic.SqlServer.Parsing
{
    internal static class SqlServerTextParsers
    {
        public static TextParser<string> SqlBlob { get; } =
            Span.EqualTo("0x")
                .IgnoreThen(Numerics.HexDigits.Select(static _ => _.ToStringValue()));

        public static TextParser<char> SqlStringContentChar { get; } =
            Span.EqualTo("''").Value('\'').Try().Or(Character.ExceptIn('\''));

        public static TextParser<string> SqlString { get; } =
            Character.EqualToIgnoreCase('N').IgnoreThen(Character.EqualTo('\''))
                .Try().Or(Character.EqualTo('\''))
                .IgnoreThen(SqlStringContentChar.Many())
                .Then(static s => Character.EqualTo('\'').Value(new string(s)));

        public static TextParser<string> SqlComment { get; } =
            Comment.CStyle.Or(Comment.SqlStyle).Select(static _ => _.ToStringValue());

        public static TextParser<TextSpan> Real { get; } =
            Numerics.Decimal;

        public static TextParser<TextSpan> Money { get; } =
            Character.EqualTo('$')
                .IgnoreThen(Numerics.Decimal);

        private static readonly TextParser<SqlServerToken> LessOrEqual = Span.EqualTo("<=").Value(SqlServerToken.LessThanOrEqual);
        private static readonly TextParser<SqlServerToken> GreaterOrEqual = Span.EqualTo(">=").Value(SqlServerToken.GreaterThanOrEqual);
        private static readonly TextParser<SqlServerToken> NotEqual = Span.EqualTo("<>").Value(SqlServerToken.NotEqual);
        private static readonly TextParser<SqlServerToken> PlusEqual = Span.EqualTo("+=").Value(SqlServerToken.PlusEquals);
        private static readonly TextParser<SqlServerToken> MinusEqual = Span.EqualTo("-=").Value(SqlServerToken.MinusEquals);
        private static readonly TextParser<SqlServerToken> MultiplyEqual = Span.EqualTo("*=").Value(SqlServerToken.MultiplyEquals);
        private static readonly TextParser<SqlServerToken> DivideEqual = Span.EqualTo("/=").Value(SqlServerToken.DivideEquals);
        private static readonly TextParser<SqlServerToken> NonStandardNotEqual = Span.EqualTo("!=").Value(SqlServerToken.NonStandardNotEqual);
        private static readonly TextParser<SqlServerToken> NotLessThan = Span.EqualTo("!<").Value(SqlServerToken.NonStandardNotLessThan);
        private static readonly TextParser<SqlServerToken> NotGreaterThan = Span.EqualTo("!>").Value(SqlServerToken.NonStandardNotGreaterThan);
        private static readonly TextParser<SqlServerToken> BitwiseAndEqual = Span.EqualTo("&=").Value(SqlServerToken.BitwiseAndEqual);
        private static readonly TextParser<SqlServerToken> BitwiseOrEqual = Span.EqualTo("|=").Value(SqlServerToken.BitwiseOrEqual);
        private static readonly TextParser<SqlServerToken> BitwiseXorEqual = Span.EqualTo("^=").Value(SqlServerToken.BitwiseXorEqual);
        private static readonly TextParser<SqlServerToken> Scope = Span.EqualTo("::").Value(SqlServerToken.Scope);

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
