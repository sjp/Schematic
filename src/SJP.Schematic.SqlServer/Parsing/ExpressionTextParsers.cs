using System;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace SJP.Schematic.SqlServer.Parsing
{
    internal static class ExpressionTextParsers
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

        public static TextParser<TextSpan> Real { get; } =
            Numerics.Integer
                .Then(n => Character.EqualTo('.').IgnoreThen(Numerics.Integer).OptionalOrDefault()
                    .Select(f => f == TextSpan.None ? n : new TextSpan(n.Source, n.Position, n.Length + f.Length + 1)));

        public static TextParser<TextSpan> Money { get; } =
            Character.EqualTo('$')
                .IgnoreThen(Numerics.Integer
                    .Then(n => Character.EqualTo('.').IgnoreThen(Numerics.Integer).OptionalOrDefault()
                        .Select(f => f == TextSpan.None ? n : new TextSpan(n.Source, n.Position, n.Length + f.Length + 1))));

        private static readonly TextParser<ExpressionToken> LessOrEqual = Span.EqualTo("<=").Value(ExpressionToken.LessThanOrEqual);
        private static readonly TextParser<ExpressionToken> GreaterOrEqual = Span.EqualTo(">=").Value(ExpressionToken.GreaterThanOrEqual);
        private static readonly TextParser<ExpressionToken> NotEqual = Span.EqualTo("<>").Value(ExpressionToken.NotEqual);
        private static readonly TextParser<ExpressionToken> PlusEqual = Span.EqualTo("+=").Value(ExpressionToken.PlusEquals);
        private static readonly TextParser<ExpressionToken> MinusEqual = Span.EqualTo("-=").Value(ExpressionToken.MinusEquals);
        private static readonly TextParser<ExpressionToken> MultiplyEqual = Span.EqualTo("*=").Value(ExpressionToken.MultiplyEquals);
        private static readonly TextParser<ExpressionToken> DivideEqual = Span.EqualTo("/=").Value(ExpressionToken.DivideEquals);
        private static readonly TextParser<ExpressionToken> NonStandardNotEqual = Span.EqualTo("!=").Value(ExpressionToken.NonStandardNotEqual);
        private static readonly TextParser<ExpressionToken> NotLessThan = Span.EqualTo("!<").Value(ExpressionToken.NonStandardNotLessThan);
        private static readonly TextParser<ExpressionToken> NotGreaterThan = Span.EqualTo("!>").Value(ExpressionToken.NonStandardNotGreaterThan);
        private static readonly TextParser<ExpressionToken> BitwiseAndEqual = Span.EqualTo("&=").Value(ExpressionToken.BitwiseAndEqual);
        private static readonly TextParser<ExpressionToken> BitwiseOrEqual = Span.EqualTo("|=").Value(ExpressionToken.BitwiseOrEqual);
        private static readonly TextParser<ExpressionToken> BitwiseXorEqual = Span.EqualTo("^=").Value(ExpressionToken.BitwiseXorEqual);
        private static readonly TextParser<ExpressionToken> Scope = Span.EqualTo("::").Value(ExpressionToken.Scope);

        public static TextParser<ExpressionToken> CompoundOperator { get; } =
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
