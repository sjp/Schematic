using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Parsing;

namespace SJP.Schematic.Sqlite
{
    public sealed class SqliteDependencyProvider : IDependencyProvider
    {
        public SqliteDependencyProvider(IEqualityComparer<Identifier>? comparer = null)
        {
            Comparer = comparer ?? IdentifierComparer.OrdinalIgnoreCase;
        }

        private IEqualityComparer<Identifier> Comparer { get; }

        public IReadOnlyCollection<Identifier> GetDependencies(Identifier objectName, string expression)
        {
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));
            if (expression.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(expression));

            var tokenizer = new SqliteTokenizer();

            var tokenizeResult = tokenizer.TryTokenize(expression);
            if (!tokenizeResult.HasValue)
                throw new ArgumentException($"Could not parse the given expression as a SQL expression. Given: { expression }", nameof(expression));

            var result = new HashSet<Identifier>(Comparer);

            var tokens = tokenizeResult.Value;

            var next = tokens.ConsumeToken();
            while (next.HasValue)
            {
                var sqlIdentifier = SqliteTokenParsers.QualifiedName(next.Location);
                if (sqlIdentifier.HasValue)
                {
                    var dependentIdentifier = sqlIdentifier.Value;
                    if (!Comparer.Equals(dependentIdentifier.Value, objectName))
                        result.Add(dependentIdentifier.Value);

                    next = sqlIdentifier.Remainder.ConsumeToken();
                }
                else
                {
                    next = next.Remainder.ConsumeToken();
                }
            }

            return result;
        }
    }
}
