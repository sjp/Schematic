using System;
using System.Collections.Generic;
using EnumsNET;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core;

/// <summary>
/// Represents an <see cref="Identifier"/> comparison operation that uses specific case and culture-based or ordinal comparison rules.
/// </summary>
public sealed class IdentifierComparer : IEqualityComparer<Identifier>, IComparer<Identifier>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdentifierComparer"/> class.
    /// </summary>
    /// <param name="comparison">The string comparison method to use.</param>
    /// <param name="defaultServer">The default server to use when missing in an <see cref="Identifier"/>.</param>
    /// <param name="defaultDatabase">The default database to use when missing in an <see cref="Identifier"/>.</param>
    /// <param name="defaultSchema">The default schema to use when missing in an <see cref="Identifier"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="comparison"/> is an invalid value.</exception>
    public IdentifierComparer(StringComparison comparison = StringComparison.OrdinalIgnoreCase, string? defaultServer = null, string? defaultDatabase = null, string? defaultSchema = null)
    {
        if (!comparison.IsValid())
            throw new ArgumentException($"The { nameof(StringComparison) } provided must be a valid enum.", nameof(comparison));

        _comparer = StringComparerLookup[comparison];

        _defaultServer = defaultServer.IsNullOrWhiteSpace() ? null : defaultServer;
        _defaultDatabase = defaultDatabase.IsNullOrWhiteSpace() ? null : defaultDatabase;
        _defaultSchema = defaultSchema.IsNullOrWhiteSpace() ? null : defaultSchema;

        _defaultServerHash = _defaultServer != null ? _comparer.GetHashCode(_defaultServer) : 0;
        _defaultDatabaseHash = _defaultDatabase != null ? _comparer.GetHashCode(_defaultDatabase) : 0;
        _defaultSchemaHash = _defaultSchema != null ? _comparer.GetHashCode(_defaultSchema) : 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentifierComparer"/> class.
    /// </summary>
    /// <param name="comparer">A string comparer to apply to each component of an <see cref="Identifier"/>.</param>
    /// <param name="defaultServer">The default server to use when missing in an <see cref="Identifier"/>.</param>
    /// <param name="defaultDatabase">The default database to use when missing in an <see cref="Identifier"/>.</param>
    /// <param name="defaultSchema">The default schema to use when missing in an <see cref="Identifier"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is <c>null</c>.</exception>
    public IdentifierComparer(StringComparer comparer, string? defaultServer = null, string? defaultDatabase = null, string? defaultSchema = null) // can't use IComparer or IEqualityComparer because we need both
    {
        _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

        _defaultServer = defaultServer.IsNullOrWhiteSpace() ? null : defaultServer;
        _defaultDatabase = defaultDatabase.IsNullOrWhiteSpace() ? null : defaultDatabase;
        _defaultSchema = defaultSchema.IsNullOrWhiteSpace() ? null : defaultSchema;

        _defaultServerHash = _defaultServer != null ? _comparer.GetHashCode(_defaultServer) : 0;
        _defaultDatabaseHash = _defaultDatabase != null ? _comparer.GetHashCode(_defaultDatabase) : 0;
        _defaultSchemaHash = _defaultSchema != null ? _comparer.GetHashCode(_defaultSchema) : 0;
    }

    /// <summary>
    /// Determines whether two <see cref="Identifier"/> instances are equal.
    /// </summary>
    /// <param name="x">The first <see cref="Identifier"/> instance to compare.</param>
    /// <param name="y">The second <see cref="Identifier"/> instance to compare.</param>
    /// <returns><see langword="true" /> if the <paramref name="x"/> and <paramref name="y"/> are equal; otherwise, <see langword="false" />.</returns>
    public bool Equals(Identifier? x, Identifier? y)
    {
        if (x is null && y is null)
            return true;
        if (x is null || y is null)
            return false;

        // both must be not null at this point
        return _comparer.Equals(x.Server ?? _defaultServer, y.Server ?? _defaultServer)
            && _comparer.Equals(x.Database ?? _defaultDatabase, y.Database ?? _defaultDatabase)
            && _comparer.Equals(x.Schema ?? _defaultSchema, y.Schema ?? _defaultSchema)
            && _comparer.Equals(x.LocalName, y.LocalName);
    }

    /// <summary>
    /// Returns a hash code for an <see cref="Identifier"/> instance.
    /// </summary>
    /// <param name="obj">An <see cref="Identifier"/>.</param>
    /// <returns>A hash code for an <see cref="Identifier"/>, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public int GetHashCode(Identifier obj)
    {
        if (obj == null)
            return 0;

        return HashCode.Combine(
            obj.Server != null ? _comparer.GetHashCode(obj.Server) : _defaultServerHash,
            obj.Database != null ? _comparer.GetHashCode(obj.Database) : _defaultDatabaseHash,
            obj.Schema != null ? _comparer.GetHashCode(obj.Schema) : _defaultSchemaHash,
            obj.LocalName != null ? _comparer.GetHashCode(obj.LocalName) : 0
        );
    }

    /// <summary>
    /// Compares two <see cref="Identifier"/> instances and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first <see cref="Identifier"/> to compare.</param>
    /// <param name="y">The second <see cref="Identifier"/> to compare.</param>
    /// <returns>
    /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.
    /// Value
    /// Meaning
    /// Less than zero
    /// <paramref name="x" /> is less than <paramref name="y" />.
    /// Zero
    /// <paramref name="x" /> equals <paramref name="y" />.
    /// Greater than zero
    /// <paramref name="x" /> is greater than <paramref name="y" />.
    /// </returns>
    public int Compare(Identifier? x, Identifier? y)
    {
        if (ReferenceEquals(x, y))
            return 0;
        if (x == null)
            return -1;
        if (y == null)
            return 1;

        var result = _comparer.Compare(x.Server ?? _defaultServer, y.Server ?? _defaultServer);
        if (result != 0)
            return result;

        result = _comparer.Compare(x.Database ?? _defaultDatabase, y.Database ?? _defaultDatabase);
        if (result != 0)
            return result;

        result = _comparer.Compare(x.Schema ?? _defaultSchema, y.Schema ?? _defaultSchema);
        if (result != 0)
            return result;

        return _comparer.Compare(x.LocalName, y.LocalName);
    }

    /// <summary>
    /// Gets a <see cref="IdentifierComparer"/> object that performs a case-sensitive identifier comparison using the word comparison rules of the current culture.
    /// </summary>
    /// <value>A <see cref="IdentifierComparer"/> object.</value>
    public static IdentifierComparer CurrentCulture { get; } = new IdentifierComparer(StringComparer.CurrentCulture);

    /// <summary>
    /// Gets a <see cref="IdentifierComparer"/> object that performs a case-insensitive identifier comparison using the word comparison rules of the current culture.
    /// </summary>
    /// <value>A <see cref="IdentifierComparer"/> object.</value>
    public static IdentifierComparer CurrentCultureIgnoreCase { get; } = new IdentifierComparer(StringComparer.CurrentCultureIgnoreCase);

    /// <summary>
    /// Gets a <see cref="IdentifierComparer"/> object that performs a case-sensitive identifier comparison.
    /// </summary>
    /// <value>A <see cref="IdentifierComparer"/> object.</value>
    public static IdentifierComparer Ordinal { get; } = new IdentifierComparer(StringComparer.Ordinal);

    /// <summary>
    /// Gets a <see cref="IdentifierComparer"/> object that performs a case-insensitive identifier comparison.
    /// </summary>
    /// <value>A <see cref="IdentifierComparer"/> object.</value>
    public static IdentifierComparer OrdinalIgnoreCase { get; } = new IdentifierComparer(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a <see cref="IdentifierComparer"/> object that performs a case-sensitive identifier comparison using the word comparison rules of the invariant culture.
    /// </summary>
    /// <value>A <see cref="IdentifierComparer"/> object.</value>
    public static IdentifierComparer InvariantCulture { get; } = new IdentifierComparer(StringComparer.InvariantCulture);

    /// <summary>
    /// Gets a <see cref="IdentifierComparer"/> object that performs a case-insensitive identifier comparison using the word comparison rules of the invariant culture.
    /// </summary>
    /// <value>A <see cref="IdentifierComparer"/> object.</value>
    public static IdentifierComparer InvariantCultureIgnoreCase { get; } = new IdentifierComparer(StringComparer.InvariantCultureIgnoreCase);

    private readonly string? _defaultSchema;
    private readonly string? _defaultDatabase;
    private readonly string? _defaultServer;
    private readonly int _defaultSchemaHash;
    private readonly int _defaultDatabaseHash;
    private readonly int _defaultServerHash;
    private readonly StringComparer _comparer;

    private static readonly IReadOnlyDictionary<StringComparison, StringComparer> StringComparerLookup = new Dictionary<StringComparison, StringComparer>
    {
        [StringComparison.CurrentCulture] = StringComparer.CurrentCulture,
        [StringComparison.CurrentCultureIgnoreCase] = StringComparer.CurrentCultureIgnoreCase,
        [StringComparison.Ordinal] = StringComparer.Ordinal,
        [StringComparison.OrdinalIgnoreCase] = StringComparer.OrdinalIgnoreCase,
        [StringComparison.InvariantCulture] = StringComparer.InvariantCulture,
        [StringComparison.InvariantCultureIgnoreCase] = StringComparer.InvariantCultureIgnoreCase
    };
}
