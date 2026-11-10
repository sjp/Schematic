using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Tests.Utilities;

/// <summary>
/// Key constraints for tests that need a key but do not exercise one, so that a test says what
/// it is about rather than mocking or spelling out a key definition.
/// </summary>
/// <remarks>
/// Every key is enabled, and unnamed unless a name is given. A key built without columns covers
/// <see cref="Column"/>, so that a foreign key and the primary or unique key it refers to always
/// have matching column counts.
/// </remarks>
public static class TestKeys
{
    /// <summary>
    /// The column covered by a key that is built without any columns.
    /// </summary>
    public static IDatabaseColumn Column { get; } = new DatabaseColumn(
        "test_key_column",
        TestDbTypes.BigInteger,
        false,
        Option<IDatabaseDefaultValue>.None,
        Option<IAutoIncrement>.None
    );

    /// <summary>
    /// Creates an unnamed, enabled primary key.
    /// </summary>
    /// <param name="columns">The key columns. When none are given, the key covers <see cref="Column"/>.</param>
    /// <returns>A primary key.</returns>
    public static DatabaseKey.Primary Primary(params IReadOnlyCollection<IDatabaseColumn> columns)
        => new(Option<Identifier>.None, KeyColumns(columns), true);

    /// <summary>
    /// Creates an enabled primary key with the given name.
    /// </summary>
    /// <param name="name">The key name, if any.</param>
    /// <param name="columns">The key columns. When none are given, the key covers <see cref="Column"/>.</param>
    /// <returns>A primary key.</returns>
    public static DatabaseKey.Primary Primary(Option<Identifier> name, params IReadOnlyCollection<IDatabaseColumn> columns)
        => new(name, KeyColumns(columns), true);

    /// <summary>
    /// Creates an unnamed, enabled unique key.
    /// </summary>
    /// <param name="columns">The key columns. When none are given, the key covers <see cref="Column"/>.</param>
    /// <returns>A unique key.</returns>
    public static DatabaseKey.Unique Unique(params IReadOnlyCollection<IDatabaseColumn> columns)
        => new(Option<Identifier>.None, KeyColumns(columns), true);

    /// <summary>
    /// Creates an enabled unique key with the given name.
    /// </summary>
    /// <param name="name">The key name, if any.</param>
    /// <param name="columns">The key columns. When none are given, the key covers <see cref="Column"/>.</param>
    /// <returns>A unique key.</returns>
    public static DatabaseKey.Unique Unique(Option<Identifier> name, params IReadOnlyCollection<IDatabaseColumn> columns)
        => new(name, KeyColumns(columns), true);

    /// <summary>
    /// Creates an unnamed, enabled foreign key.
    /// </summary>
    /// <param name="columns">The key columns. When none are given, the key covers <see cref="Column"/>.</param>
    /// <returns>A foreign key.</returns>
    public static DatabaseKey.Foreign Foreign(params IReadOnlyCollection<IDatabaseColumn> columns)
        => new(Option<Identifier>.None, KeyColumns(columns), true);

    /// <summary>
    /// Creates an enabled foreign key with the given name.
    /// </summary>
    /// <param name="name">The key name, if any.</param>
    /// <param name="columns">The key columns. When none are given, the key covers <see cref="Column"/>.</param>
    /// <returns>A foreign key.</returns>
    public static DatabaseKey.Foreign Foreign(Option<Identifier> name, params IReadOnlyCollection<IDatabaseColumn> columns)
        => new(name, KeyColumns(columns), true);

    private static IReadOnlyCollection<IDatabaseColumn> KeyColumns(IReadOnlyCollection<IDatabaseColumn> columns)
        => columns.Count > 0 ? columns : [Column];
}
