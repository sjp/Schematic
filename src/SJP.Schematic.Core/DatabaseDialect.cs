using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core;

/// <summary>
/// A generic implementation of a database dialect.
/// </summary>
/// <seealso cref="IDatabaseDialect" />
public abstract class DatabaseDialect : IDatabaseDialect
{
    /// <summary>
    /// Quotes a qualified name.
    /// </summary>
    /// <param name="name">An object name.</param>
    /// <returns>A quoted name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null" />.</exception>
    public virtual string QuoteName(Identifier name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var pieces = new List<string>();

        if (name.Server != null)
            pieces.Add(QuoteIdentifier(name.Server));
        if (name.Database != null)
            pieces.Add(QuoteIdentifier(name.Database));
        if (name.Schema != null)
            pieces.Add(QuoteIdentifier(name.Schema));
        if (name.LocalName != null)
            pieces.Add(QuoteIdentifier(name.LocalName));

        return pieces.Join(".");
    }

    /// <summary>
    /// Quotes a string identifier, e.g. a column name.
    /// </summary>
    /// <param name="identifier">An identifier.</param>
    /// <returns>A quoted identifier.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is <see langword="null" />.</exception>
    public virtual string QuoteIdentifier(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

        return $"\"{identifier.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }

    /// <summary>
    /// Determines whether the given text is a reserved keyword.
    /// </summary>
    /// <param name="text">A piece of text.</param>
    /// <returns><see langword="true" /> if the given text is a reserved keyword; otherwise, <see langword="false" />.</returns>
    public abstract bool IsReservedKeyword(string text);

    /// <summary>
    /// Gets a database column data type provider.
    /// </summary>
    /// <value>The type provider.</value>
    public abstract IDbTypeProvider TypeProvider { get; }

    /// <summary>
    /// Gets a dependency provider, that always returns no values.
    /// </summary>
    /// <returns>A dependency provider that always returns no values.</returns>
    public virtual IDependencyProvider GetDependencyProvider() => _emptyDependencyProvider;

    private static readonly IDependencyProvider _emptyDependencyProvider = new EmptyDependencyProvider();
}