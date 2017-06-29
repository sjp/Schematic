using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using SJP.Schema.Core;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace SJP.Schema.Modelled.Reflection.Model
{
    // TODO:
    // need to scope this to the reflection namespace
    // because we want something like sql server expressions...
    public class ModelledSqlExpression : ISqlExpression
    {
        public ModelledSqlExpression(string expression, object param)
        {
            if (expression.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(expression));

            ExpressionText = expression;

            var parser = new ExpressionParser();
            Tokens = parser.Tokenize(expression).ToImmutableList();
            var lookup = ObjectToDictionary(param);

            // TODO: check unbound names -- definitely an error
            //       also check that there are extra names in the object

            var tokenVarNames = new HashSet<string>(Tokens.Select(UnwrapTokenValue));
            var lookupNames = new HashSet<string>(lookup.Keys);

            var unboundVariables = tokenVarNames.Where(v => !lookupNames.Contains(v));
            if (unboundVariables.Any())
                throw new ArgumentException("The expression contains references to the following variables which are not present in the parameter object: " + unboundVariables.Join(", "), nameof(expression));

            // TODO: maybe allow this?
            //       would allow sloppy coding though, not sure if we ever want this to occur
            var extraParams = lookupNames.Where(name => !tokenVarNames.Contains(name));
            if (extraParams.Any())
                throw new ArgumentException("The parameter object contains extra variables that are not present in the expression.", nameof(param));

            // TODO: need to implement dependency support -- maybe have a separate bind for variables or types
            //       e.g. :Column for dependency binding, can use types (if present in DB)
            //            @Value for values that need to be quoted
            //            {=Value} for values that don't need to be quoted (already handled in expression)
            // to think about...
        }

        protected string ExpressionText { get; }

        protected IReadOnlyList<Token<ExpressionToken>> Tokens { get; }

        protected IReadOnlyDictionary<string, object> Parameters { get; }

        public IEnumerable<Identifier> DependentNames
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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

            var builder = new StringBuilder(ExpressionText.Length);

            var tokenValues = GetTokenValues(dialect);
            var tokenInfo = Tokens.Zip(tokenValues, (t, v) =>
            {
                var startPos = t.Position.Absolute;
                var endPos = startPos + t.Span.Length;
                return new { Start = startPos, End = endPos, Value = v };
            }).ToList();

            var firstInfo = tokenInfo[0];
            if (firstInfo.Start > 0)
                builder.Append(ExpressionText.Substring(0, firstInfo.Start));

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

            return builder.ToString();
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
                        // TODO: get rid of the modelled column from here
                        //       derive if we need to
                        //       should be common to sqlserver, sqlite, reflection, json, etc
                        var stringVal = variableValue as string;
                        if (columnObject != null)
                        {
                            // also get name properly anyway...
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

        private static IReadOnlyDictionary<string, object> EmptyLookup { get; } = new Dictionary<string, object>().ToImmutableDictionary();

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

            return result.ToImmutableDictionary();
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

        protected class ExpressionParser : Tokenizer<ExpressionToken>
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

            private static readonly ISet<UnicodeCategory> _validIdentifierCategories =
                new HashSet<UnicodeCategory>(new[] { UnicodeCategory.ConnectorPunctuation, UnicodeCategory.Format, UnicodeCategory.SpacingCombiningMark, UnicodeCategory.NonSpacingMark });
        }
    }

    // TODO make this take a column?
    // means we can retrieve the column later and ensure any names are overridden
    // maybe just replace with a new expression in the table/view layer as it will know the column
    public static class Sql
    {
        public static ModelledSqlExpression Identity(ModelledColumn column) => new ModelledSqlExpression(IdentityFormat, ToParamObject(column));

        public static ModelledSqlExpression Identity(Identifier name) => new ModelledSqlExpression(IdentityFormat, ToParamObject(name));

        public static ModelledSqlExpression Lower(ModelledColumn column) => new ModelledSqlExpression(LowerFormat, ToParamObject(column));

        public static ModelledSqlExpression Lower(Identifier name) => new ModelledSqlExpression(LowerFormat, ToParamObject(name));

        public static ModelledSqlExpression Upper(ModelledColumn column) => new ModelledSqlExpression(UpperFormat, ToParamObject(column));

        public static ModelledSqlExpression Upper(Identifier name) => new ModelledSqlExpression(UpperFormat, name);

        public static ModelledSqlExpression Coalesce(ModelledColumn column, IConvertible value) => new ModelledSqlExpression(CoalesceFormat, new { Identity = column, Value = value });

        public static ModelledSqlExpression Coalesce(Identifier name, IConvertible value) => new ModelledSqlExpression(CoalesceFormat, new { Identity = name, Value = value });

        private static object ToParamObject(object obj) => new { Identity = obj };

        private static string IdentityFormat { get; } = "@Identity";

        private static string LowerFormat { get; } = "LOWER(@Identity)";

        private static string UpperFormat { get; } = "UPPER(@Identity)";

        private static string CoalesceFormat { get; } = "COALESCE(@Identity, @Value)";
    }
}
