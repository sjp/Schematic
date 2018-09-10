using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using SchematicIdentifier = SJP.Schematic.Core.Identifier;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public class ModelledSqlExpression : IModelledSqlExpression
    {
        public ModelledSqlExpression(string expression, object param)
        {
            if (expression.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(expression));

            ExpressionText = expression;

            var parser = new ExpressionParser();
            Tokens = parser.Tokenize(expression).ToList();
            var lookup = ObjectToDictionary(param);

            var tokenVarNames = new HashSet<string>(Tokens.Select(UnwrapTokenValue));
            var lookupNames = new HashSet<string>(lookup.Keys);

            var unboundVariables = tokenVarNames.Where(v => !lookupNames.Contains(v));
            if (unboundVariables.Any())
                throw new ArgumentException("The expression contains references to the following variables which are not present in the parameter object: " + unboundVariables.Join(", "), nameof(expression));
        }

        protected string ExpressionText { get; }

        protected IReadOnlyList<Token<ExpressionToken>> Tokens { get; }

        protected IReadOnlyDictionary<string, object> Parameters { get; }

        public IEnumerable<SchematicIdentifier> DependentNames { get; } = Array.Empty<SchematicIdentifier>();

        public bool IsIdentity
        {
            get
            {
                if (Tokens.Count != 1)
                    return false;

                var token = Tokens.Single();
                return token.ToStringValue() == ExpressionText;
            }
        }

        public string ToSql(IDatabaseDialect dialect)
        {
            if (Tokens.Empty())
                return ExpressionText;

            var builder = StringBuilderCache.Acquire(ExpressionText.Length);

            var tokenValues = GetTokenValues(dialect);
            var tokenInfo = Tokens.Zip(tokenValues, (t, v) =>
            {
                var startPos = t.Position.Absolute;
                var endPos = startPos + t.Span.Length;
                return new { Start = startPos, End = endPos, Value = v };
            }).ToList();

            var firstInfo = tokenInfo[0];
            if (firstInfo.Start > 0)
                builder.Append(ExpressionText, 0, firstInfo.Start);

            var prevEnd = -1;
            foreach (var info in tokenInfo)
            {
                if (prevEnd < info.Start)
                {
                    prevEnd = Math.Max(0, prevEnd);
                    var text = ExpressionText.Substring(prevEnd, info.Start - prevEnd);
                    builder.Append(text);
                }

                builder.Append(info.Value);
            }

            if (prevEnd < (ExpressionText.Length - 1))
                builder.Append(ExpressionText.Substring(prevEnd + 1));

            return StringBuilderCache.GetStringAndRelease(builder);
        }

        private IEnumerable<string> GetTokenValues(IDatabaseDialect dialect)
        {
            foreach (var token in Tokens)
            {
                switch (token.Kind)
                {
                    case ExpressionToken.Literal:
                        var literalValue = Parameters[UnwrapTokenValue(token)];
                        yield return literalValue.ToString();
                        break;
                    case ExpressionToken.Variable:
                        var variableValue = Parameters[UnwrapTokenValue(token)];
                        var columnObject = variableValue as IModelledColumn;

                        var stringVal = variableValue as string;
                        if (columnObject != null)
                        {
                            var columnName = dialect.GetAliasOrDefault(columnObject.Property);
                            yield return dialect.QuoteName(columnName);
                        }
                        else if (stringVal != null)
                        {
                            stringVal = "'" + stringVal.Replace("'", "''") + "'";
                            yield return stringVal;
                        }
                        else
                        {
                            throw new Exception("Cannot quote a non-column or non-string object. Attempted to quote: " + token.ToStringValue()); // should be done in the ctor...
                        }

                        break;
                    default:
                        throw new Exception("Unknown or unsupported token type " + token.Kind.ToString());
                }
            }
        }

        private static IReadOnlyDictionary<string, object> ObjectToDictionary(object param)
        {
            var result = new Dictionary<string, object>();

            var objType = param.GetType();
            var properties = objType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (properties.Empty())
                throw new ArgumentException("The given object should be an anonymous object with at least one property set", nameof(param));

            foreach (var prop in properties)
            {
                var propName = prop.Name;
                var propValue = prop.GetValue(param);
                result[propName] = propValue ?? throw new ArgumentException($"The property { propName } on the given parameter object is null. A value must be set", nameof(param));
            }

            return result;
        }

        protected static string UnwrapTokenValue(Token<ExpressionToken> token)
        {
            switch (token.Kind)
            {
                case ExpressionToken.Variable:
                    return token.ToStringValue().TrimStart('@');
                case ExpressionToken.Literal:
                    return token.ToStringValue().TrimStart('{', '=').TrimEnd('}');
                default:
                    throw new ArgumentException("Unsupported token type: " + token.Kind.ToString(), nameof(token));
            }
        }

        protected enum ExpressionToken
        {
            None,
            Variable,
            Literal
        }

        protected sealed class ExpressionParser : Tokenizer<ExpressionToken>
        {
            protected override IEnumerable<Result<ExpressionToken>> Tokenize(TextSpan span)
            {
                var next = SkipWhiteSpace(span);
                if (!next.HasValue)
                    yield break;

                do
                {
                    if (next.Value == '@')
                    {
                        var escapedAt = EscapedAtParser(next.Location);
                        if (escapedAt.HasValue)
                        {
                            next = escapedAt.Remainder.ConsumeChar();
                        }
                        else
                        {
                            var varStart = VariableParser(next.Location);
                            if (varStart.HasValue)
                            {
                                next = varStart.Remainder.ConsumeChar();
                                yield return Result.Value(ExpressionToken.Variable, varStart.Location, varStart.Remainder);
                            }
                            else
                            {
                                next = next.Remainder.ConsumeChar();
                            }
                        }
                    }
                    else if (next.Value == '{')
                    {
                        var escapedLiteral = EscapedLiteralParser(next.Location);
                        if (escapedLiteral.HasValue)
                        {
                            next = escapedLiteral.Remainder.ConsumeChar();
                        }
                        else
                        {
                            var literalStart = LiteralParser(next.Location);
                            if (literalStart.HasValue)
                            {
                                next = literalStart.Remainder.ConsumeChar();
                                yield return Result.Value(ExpressionToken.Literal, literalStart.Location, literalStart.Remainder);
                            }
                            else
                            {
                                next = next.Remainder.ConsumeChar();
                            }
                        }
                    }
                    else
                    {
                        next = next.Remainder.ConsumeChar();
                    }

                    next = SkipWhiteSpace(next.Location);
                }
                while (next.HasValue);
            }

            private static TextParser<string> EscapedLiteralParser { get; } =
                Span.EqualTo("{{").Value("{");

            private static TextParser<string> EscapedAtParser { get; } =
                Span.EqualTo("@@").Value("@");

            private static TextParser<string> VariableParser { get; } =
                Span.EqualTo("@")
                    .IgnoreThen(Character.Letter.Or(Character.EqualTo('_')).AtLeastOnce())
                    .IgnoreThen(Character.Matching(IsValidIdentifierPartCharacter, "valid C# identifier char").Many())
                    .Select(c => new string(c));

            private static TextParser<string> LiteralParser { get; } =
                Span.EqualTo("{=")
                    .IgnoreThen(Character.Letter.Or(Character.EqualTo('_')).AtLeastOnce())
                    .IgnoreThen(Character.Matching(IsValidIdentifierPartCharacter, "valid C# identifier char").Many())
                    .Then(s => Character.EqualTo('}').Value(new string(s)));

            private static bool IsValidIdentifierPartCharacter(char c) =>
                c.IsLetterOrDigit() || _validIdentifierCategories.Contains(c.GetUnicodeCategory());

            private readonly static IEnumerable<UnicodeCategory> _validIdentifierCategories =
                new HashSet<UnicodeCategory>(new[] { UnicodeCategory.ConnectorPunctuation, UnicodeCategory.Format, UnicodeCategory.SpacingCombiningMark, UnicodeCategory.NonSpacingMark });
        }
    }

    public static class Sql
    {
        public static ModelledSqlExpression Identity(ModelledColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return new ModelledSqlExpression(IdentityFormat, ToParamObject(column));
        }

        public static ModelledSqlExpression Identity(SchematicIdentifier name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return new ModelledSqlExpression(IdentityFormat, ToParamObject(name));
        }

        public static ModelledSqlExpression Lower(ModelledColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return new ModelledSqlExpression(LowerFormat, ToParamObject(column));
        }

        public static ModelledSqlExpression Lower(SchematicIdentifier name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return new ModelledSqlExpression(LowerFormat, ToParamObject(name));
        }

        public static ModelledSqlExpression Upper(ModelledColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return new ModelledSqlExpression(UpperFormat, ToParamObject(column));
        }

        public static ModelledSqlExpression Upper(SchematicIdentifier name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return new ModelledSqlExpression(UpperFormat, name);
        }

        public static ModelledSqlExpression Coalesce(ModelledColumn column, IConvertible value)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return new ModelledSqlExpression(CoalesceFormat, new { Identity = column, Value = value });
        }

        public static ModelledSqlExpression Coalesce(SchematicIdentifier name, IConvertible value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return new ModelledSqlExpression(CoalesceFormat, new { Identity = name, Value = value });
        }

        private static object ToParamObject(object obj) => new { Identity = obj };

        private const string IdentityFormat = "@Identity";

        private const string LowerFormat = "LOWER(@Identity)";

        private const string UpperFormat = "UPPER(@Identity)";

        private const string CoalesceFormat = "COALESCE(@Identity, @Value)";
    }
}
