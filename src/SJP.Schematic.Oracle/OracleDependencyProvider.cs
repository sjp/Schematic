using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Parsing;

namespace SJP.Schematic.Oracle
{
    /// <summary>
    /// A dependency provider for Oracle database objects.
    /// </summary>
    /// <seealso cref="IDependencyProvider" />
    public sealed class OracleDependencyProvider : IDependencyProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleDependencyProvider"/> class.
        /// </summary>
        /// <param name="comparer">A comparer.</param>
        public OracleDependencyProvider(IEqualityComparer<Identifier>? comparer = null)
        {
            Comparer = comparer ?? IdentifierComparer.Ordinal;
        }

        private IEqualityComparer<Identifier> Comparer { get; }

        /// <summary>
        /// Retrieves all dependencies for an expression.
        /// </summary>
        /// <param name="objectName">The name of an object defined by an expression (e.g. a computed column definition).</param>
        /// <param name="expression">A SQL expression that may contain dependent object names.</param>
        /// <returns>A collection of identifiers found in the expression.</returns>
        /// <remarks>This will also return unqualified identifiers, which may cause ambiguity between object names and column names. Additionally it may return other identifiers, such as aliases or type names.</remarks>
        public IReadOnlyCollection<Identifier> GetDependencies(Identifier objectName, string expression)
        {
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));
            if (expression.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(expression));

            var tokenizer = new OracleTokenizer();

            var tokenizeResult = tokenizer.TryTokenize(expression);
            if (!tokenizeResult.HasValue)
                throw new ArgumentException($"Could not parse the given expression as a SQL expression. Given: { expression }", nameof(expression));

            var result = new HashSet<Identifier>(Comparer);

            var tokens = tokenizeResult.Value;

            var next = tokens.ConsumeToken();
            while (next.HasValue)
            {
                var sqlIdentifier = OracleTokenParsers.QualifiedName(next.Location);
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
