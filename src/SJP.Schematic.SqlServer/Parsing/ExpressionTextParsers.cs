using System;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace SJP.Schematic.SqlServer.Parsing
{
    internal static class ExpressionTextParsers
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

        private static readonly TextParser<ExpressionToken> LessOrEqual = Span.EqualTo("<=").Value(ExpressionToken.LessThanOrEqual);
        private static readonly TextParser<ExpressionToken> GreaterOrEqual = Span.EqualTo(">=").Value(ExpressionToken.GreaterThanOrEqual);
        private static readonly TextParser<ExpressionToken> NotEqual = Span.EqualTo("<>").Value(ExpressionToken.NotEqual);

        public static TextParser<ExpressionToken> CompoundOperator { get; } = GreaterOrEqual.Or(LessOrEqual.Try().Or(NotEqual));
    }
}
