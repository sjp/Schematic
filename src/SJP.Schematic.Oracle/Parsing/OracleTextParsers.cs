using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace SJP.Schematic.Oracle.Parsing;

internal static class OracleTextParsers
{
    public static TextParser<string> SqlBlob { get; } =
        Span.EqualTo("0x")
            .IgnoreThen(Numerics.HexDigits.Select(static _ => _.ToStringValue()));

    public static TextParser<string> SquareQuotedString { get; } =
        Character.EqualToIgnoreCase('q').IgnoreThen(Span.EqualTo("'["))
            .IgnoreThen(Character.Except(']').Many())
            .Then(static s => Span.EqualTo("]'").Value(new string(s)));

    public static TextParser<string> BraceQuotedString { get; } =
        Character.EqualToIgnoreCase('q').IgnoreThen(Span.EqualTo("'{"))
            .IgnoreThen(Character.Except('}').Many())
            .Then(static s => Span.EqualTo("}'").Value(new string(s)));

    public static TextParser<string> ParenQuotedString { get; } =
        Character.EqualToIgnoreCase('q').IgnoreThen(Span.EqualTo("'("))
            .IgnoreThen(Character.Except(')').Many())
            .Then(static s => Span.EqualTo(")'").Value(new string(s)));

    public static TextParser<string> AngleQuotedString { get; } =
        Character.EqualToIgnoreCase('q').IgnoreThen(Span.EqualTo("'<"))
            .IgnoreThen(Character.Except('>').Many())
            .Then(static s => Span.EqualTo(">'").Value(new string(s)));

    public static TextParser<string> GenericQuotedString { get; } =
        Character.EqualToIgnoreCase('q').IgnoreThen(Character.EqualTo('\''))
            .IgnoreThen(Character.AnyChar)
            .Then(static escapeChar => Character.Except(escapeChar).Many())
            .Then(static s => Character.EqualTo('\'').Value(new string(s)));

    public static TextParser<char> SqlStringContentChar { get; } =
        Span.EqualTo("''").Value('\'').Try().Or(Character.ExceptIn('\''));

    public static TextParser<string> SqlString { get; } =
        Character.EqualTo('\'')
            .IgnoreThen(SqlStringContentChar.Many())
            .Then(static s => Character.EqualTo('\'').Value(new string(s)));

    public static TextParser<string> OracleString { get; } =
        SquareQuotedString
            .Try().Or(BraceQuotedString)
            .Try().Or(ParenQuotedString)
            .Try().Or(AngleQuotedString)
            .Try().Or(GenericQuotedString)
            .Try().Or(SqlString);

    public static TextParser<string> SqlComment { get; } =
        Comment.CStyle.Or(Comment.SqlStyle).Select(static _ => _.ToStringValue());

    public static TextParser<TextSpan> Integer { get; } = Numerics.Integer;

    public static TextParser<TextSpan> Real { get; } = Numerics.Decimal;

    private static readonly TextParser<OracleToken> StringConcat = Span.EqualTo("||").Value(OracleToken.StringConcat);
    private static readonly TextParser<OracleToken> LessOrEqual = Span.EqualTo("<=").Value(OracleToken.LessThanOrEqual);
    private static readonly TextParser<OracleToken> GreaterOrEqual = Span.EqualTo(">=").Value(OracleToken.GreaterThanOrEqual);
    private static readonly TextParser<OracleToken> NotEqual = Span.EqualTo("<>").Value(OracleToken.NotEqual);
    private static readonly TextParser<OracleToken> PlusEqual = Span.EqualTo("+=").Value(OracleToken.PlusEquals);
    private static readonly TextParser<OracleToken> MinusEqual = Span.EqualTo("-=").Value(OracleToken.MinusEquals);
    private static readonly TextParser<OracleToken> MultiplyEqual = Span.EqualTo("*=").Value(OracleToken.MultiplyEquals);
    private static readonly TextParser<OracleToken> DivideEqual = Span.EqualTo("/=").Value(OracleToken.DivideEquals);
    private static readonly TextParser<OracleToken> NonStandardNotEqual = Span.EqualTo("!=").Value(OracleToken.NonStandardNotEqual);
    private static readonly TextParser<OracleToken> NotLessThan = Span.EqualTo("!<").Value(OracleToken.NonStandardNotLessThan);
    private static readonly TextParser<OracleToken> NotGreaterThan = Span.EqualTo("!>").Value(OracleToken.NonStandardNotGreaterThan);
    private static readonly TextParser<OracleToken> BitwiseAndEqual = Span.EqualTo("&=").Value(OracleToken.BitwiseAndEqual);
    private static readonly TextParser<OracleToken> BitwiseOrEqual = Span.EqualTo("|=").Value(OracleToken.BitwiseOrEqual);
    private static readonly TextParser<OracleToken> BitwiseXorEqual = Span.EqualTo("^=").Value(OracleToken.BitwiseXorEqual);
    private static readonly TextParser<OracleToken> Scope = Span.EqualTo("::").Value(OracleToken.Scope);
    private static readonly TextParser<OracleToken> Association = Span.EqualTo("=>").Value(OracleToken.Association);
    private static readonly TextParser<OracleToken> Assignment = Span.EqualTo(":=").Value(OracleToken.Assignment);
    private static readonly TextParser<OracleToken> Range = Span.EqualTo("..").Value(OracleToken.Range);

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
            .Try().Or(Scope)
            .Try().Or(Association)
            .Try().Or(Assignment)
            .Try().Or(Range);
}
