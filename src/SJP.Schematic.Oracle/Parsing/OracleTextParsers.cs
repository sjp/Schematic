using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace SJP.Schematic.Oracle.Parsing
{
    internal static class OracleTextParsers
    {
        public static TextParser<string> SqlBlob { get; } =
            Span.EqualTo("0x")
                .IgnoreThen(Numerics.HexDigits.Select(_ => _.ToStringValue()));

        public static TextParser<char> SqlStringContentChar { get; } =
            Span.EqualTo("''").Value('\'').Try().Or(Character.ExceptIn('\''));

        public static TextParser<string> SqlString { get; } =
            Character.EqualToIgnoreCase('N').IgnoreThen(Character.EqualTo('\''))
                .Try().Or(Character.EqualTo('\''))
                .IgnoreThen(SqlStringContentChar.Many())
                .Then(s => Character.EqualTo('\'').Value(new string(s)));

        public static TextParser<string> SqlComment { get; } =
            Comment.CStyle.Or(Comment.SqlStyle).Select(_ => _.ToStringValue());

        public static TextParser<TextSpan> Real { get; } =
            Numerics.Decimal;

        public static TextParser<TextSpan> Money { get; } =
            Character.EqualTo('$')
                .IgnoreThen(Numerics.Decimal);

        private readonly static TextParser<OracleToken> StringConcat = Span.EqualTo("||").Value(OracleToken.StringConcat);
        private readonly static TextParser<OracleToken> LessOrEqual = Span.EqualTo("<=").Value(OracleToken.LessThanOrEqual);
        private readonly static TextParser<OracleToken> GreaterOrEqual = Span.EqualTo(">=").Value(OracleToken.GreaterThanOrEqual);
        private readonly static TextParser<OracleToken> NotEqual = Span.EqualTo("<>").Value(OracleToken.NotEqual);
        private readonly static TextParser<OracleToken> PlusEqual = Span.EqualTo("+=").Value(OracleToken.PlusEquals);
        private readonly static TextParser<OracleToken> MinusEqual = Span.EqualTo("-=").Value(OracleToken.MinusEquals);
        private readonly static TextParser<OracleToken> MultiplyEqual = Span.EqualTo("*=").Value(OracleToken.MultiplyEquals);
        private readonly static TextParser<OracleToken> DivideEqual = Span.EqualTo("/=").Value(OracleToken.DivideEquals);
        private readonly static TextParser<OracleToken> NonStandardNotEqual = Span.EqualTo("!=").Value(OracleToken.NonStandardNotEqual);
        private readonly static TextParser<OracleToken> NotLessThan = Span.EqualTo("!<").Value(OracleToken.NonStandardNotLessThan);
        private readonly static TextParser<OracleToken> NotGreaterThan = Span.EqualTo("!>").Value(OracleToken.NonStandardNotGreaterThan);
        private readonly static TextParser<OracleToken> BitwiseAndEqual = Span.EqualTo("&=").Value(OracleToken.BitwiseAndEqual);
        private readonly static TextParser<OracleToken> BitwiseOrEqual = Span.EqualTo("|=").Value(OracleToken.BitwiseOrEqual);
        private readonly static TextParser<OracleToken> BitwiseXorEqual = Span.EqualTo("^=").Value(OracleToken.BitwiseXorEqual);
        private readonly static TextParser<OracleToken> Scope = Span.EqualTo("::").Value(OracleToken.Scope);

        public static TextParser<OracleToken> CompoundOperator { get; } =
            GreaterOrEqual
                .Try().Or(LessOrEqual)
                .Try().Or(StringConcat)
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
