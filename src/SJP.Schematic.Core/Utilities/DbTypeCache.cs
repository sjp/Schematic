using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core.Utilities;

/// <summary>
/// Shares column data types between columns that declare the same type, so that a database with many columns of a few types holds one type object per distinct type instead of one per column.
/// </summary>
/// <remarks>
/// <para>
/// A type is shared only when every value it exposes is identical, including the definition text, the CLR type and the CLR type name. Types are immutable, so sharing one is not observable except through reference equality.
/// </para>
/// <para>
/// Types that have an element type, a base type or enum values, or whose numeric precision is not a <see cref="NumericPrecision"/>, are always created afresh and never shared.
/// </para>
/// <para>
/// The cache holds on to the types it shares for its own lifetime. Once it holds about <see cref="Capacity"/> types it stops adding more, and types it does not already hold are created afresh. This keeps a cache owned by a long-lived object bounded.
/// </para>
/// <para>This type is safe to use from multiple threads.</para>
/// </remarks>
public sealed class DbTypeCache
{
    /// <summary>
    /// The number of types a cache created without a capacity holds before it stops adding more.
    /// </summary>
    public const int DefaultCapacity = 4096;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbTypeCache"/> class that holds up to <see cref="DefaultCapacity"/> types.
    /// </summary>
    public DbTypeCache()
        : this(DefaultCapacity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbTypeCache"/> class.
    /// </summary>
    /// <param name="capacity">The number of types to hold before no more are added. Zero disables sharing.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is negative.</exception>
    public DbTypeCache(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        Capacity = capacity;
    }

    /// <summary>
    /// The number of types held before no more are added.
    /// </summary>
    /// <value>A non-negative number of types. Concurrent callers can briefly take the count slightly past it.</value>
    public int Capacity { get; }

    /// <summary>
    /// The number of types currently held.
    /// </summary>
    /// <value>A non-negative number of types.</value>
    public int Count => Volatile.Read(ref _count);

    /// <summary>
    /// Gets a column data type with the given values, sharing a previously created instance when one with identical values is held.
    /// </summary>
    /// <param name="typeName">A type name.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="definition">The type definition in string form.</param>
    /// <param name="clrType">The .NET data type that the column maps to.</param>
    /// <param name="isFixedLength">Whether the type is a fixed length, <see langword="true" /> if fixed length; otherwise <see langword="false" />.</param>
    /// <param name="maxLength">The maximum length the column can store.</param>
    /// <param name="numericPrecision">The numeric precision.</param>
    /// <param name="collation">The collation.</param>
    /// <param name="elementType">The type of the elements of a collection type, if any.</param>
    /// <param name="enumValues">The values the type is restricted to, empty when it is not restricted.</param>
    /// <param name="baseType">The type that this type is defined in terms of, if any.</param>
    /// <param name="isUnsigned">Whether the type stores only non-negative values.</param>
    /// <param name="clrTypeName">The name of the .NET data type that the column maps to, or <see langword="null" /> to name <paramref name="clrType"/>.</param>
    /// <param name="fractionalSecondsPrecision">The number of digits kept after the decimal point in the seconds of a temporal value, if the type declares one.</param>
    /// <returns>A column data type with the given values. The same instance is returned for identical values while the cache holds it.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/>, <paramref name="definition"/>, <paramref name="clrType"/> or <paramref name="enumValues"/> is <see langword="null" />, or <paramref name="enumValues"/> contains a <see langword="null" /> value.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace, <paramref name="clrTypeName"/> is empty or whitespace, <paramref name="fractionalSecondsPrecision"/> is negative, or <paramref name="dataType"/> is not a valid enum.</exception>
    public IDbType GetOrCreate(
        Identifier typeName,
        DataType dataType,
        string definition,
        Type clrType,
        bool isFixedLength,
        int maxLength,
        Option<INumericPrecision> numericPrecision,
        Option<Identifier> collation,
        Option<IDbType> elementType,
        IReadOnlyList<string> enumValues,
        Option<IDbType> baseType,
        bool isUnsigned,
        string? clrTypeName = null,
        Option<int> fractionalSecondsPrecision = default
    )
    {
        ArgumentNullException.ThrowIfNull(enumValues);

        var precision = numericPrecision.MatchUnsafe(static np => np, static () => (INumericPrecision?)null);
        var shareable = elementType.IsNone
            && baseType.IsNone
            && enumValues.Count == 0
            && precision is null or NumericPrecision;
        if (!shareable || Capacity == 0)
        {
            return new ColumnDataType(typeName, dataType, definition, clrType, isFixedLength, maxLength, numericPrecision, collation, elementType, enumValues, baseType, isUnsigned, clrTypeName, fractionalSecondsPrecision);
        }

        var key = new DbTypeKey(
            typeName,
            dataType,
            definition,
            clrType,
            clrTypeName,
            isFixedLength,
            maxLength,
            precision?.Precision,
            precision?.Scale,
            collation.MatchUnsafe(static c => c, static () => (Identifier?)null),
            fractionalSecondsPrecision.MatchUnsafe(static p => (int?)p, static () => null),
            isUnsigned
        );

        // a held type was created from identical values, so those values were already validated
        if (_types.TryGetValue(key, out var cached))
            return cached;

        var created = new ColumnDataType(typeName, dataType, definition, clrType, isFixedLength, maxLength, numericPrecision, collation, elementType, enumValues, baseType, isUnsigned, clrTypeName, fractionalSecondsPrecision);
        if (Count >= Capacity)
            return created;

        var shared = _types.GetOrAdd(key, created);
        if (ReferenceEquals(shared, created))
            Interlocked.Increment(ref _count);

        return shared;
    }

    private readonly ConcurrentDictionary<DbTypeKey, IDbType> _types = new();
    private int _count;

    private readonly record struct DbTypeKey(
        Identifier TypeName,
        DataType DataType,
        string Definition,
        Type ClrType,
        string? ClrTypeName,
        bool IsFixedLength,
        int MaxLength,
        int? Precision,
        int? Scale,
        Identifier? Collation,
        int? FractionalSecondsPrecision,
        bool IsUnsigned
    );
}
