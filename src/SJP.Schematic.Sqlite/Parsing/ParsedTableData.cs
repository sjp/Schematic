using System;
using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Parsing;

/// <summary>
/// A data container that holds parsed table information from a SQLite <c>CREATE TABLE</c> statement.
/// </summary>
public sealed class ParsedTableData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParsedTableData"/> class.
    /// </summary>
    /// <param name="definition">The <c>CREATE TABLE</c> definition.</param>
    /// <param name="columns">Parsed columns.</param>
    /// <param name="primaryKey">A parsed primary key, if available.</param>
    /// <param name="uniqueKeys">Parsed unique keys.</param>
    /// <param name="parentKeys">Parsed parent keys.</param>
    /// <param name="checks">Parsed check constraints.</param>
    /// <param name="isWithoutRowId">Whether the table was declared <c>WITHOUT ROWID</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="definition"/>, <paramref name="columns"/>, <paramref name="uniqueKeys"/>, <paramref name="checks"/> or <paramref name="parentKeys"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace, or <paramref name="columns"/> is empty.</exception>
    public ParsedTableData(
        string definition,
        IReadOnlyCollection<Column> columns,
        Option<PrimaryKey> primaryKey,
        IReadOnlyCollection<UniqueKey> uniqueKeys,
        IReadOnlyCollection<ForeignKey> parentKeys,
        IReadOnlyCollection<Check> checks,
        bool isWithoutRowId = false
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definition);
        ArgumentNullException.ThrowIfNull(columns);
        if (columns.Empty())
            throw new ArgumentException("A table must have at least one column.", nameof(columns));

        Definition = definition;
        Columns = columns;
        PrimaryKey = primaryKey;
        UniqueKeys = uniqueKeys ?? throw new ArgumentNullException(nameof(uniqueKeys));
        Checks = checks ?? throw new ArgumentNullException(nameof(checks));
        ParentKeys = parentKeys ?? throw new ArgumentNullException(nameof(parentKeys));
        IsWithoutRowId = isWithoutRowId;
    }

    private ParsedTableData(string definition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definition);

        Definition = definition;
        PrimaryKey = Option<PrimaryKey>.None;
        Columns = [];
        UniqueKeys = [];
        Checks = [];
        ParentKeys = [];
    }

    /// <summary>
    /// The <c>CREATE TABLE</c> definition.
    /// </summary>
    /// <value>The table definition.</value>
    public string Definition { get; }

    /// <summary>
    /// Parsed column information.
    /// </summary>
    /// <value>A collection of parsed columns.</value>
    public IEnumerable<Column> Columns { get; }

    /// <summary>
    /// Parsed primary key constraint information.
    /// </summary>
    /// <value>A parsed primary key, if available.</value>
    public Option<PrimaryKey> PrimaryKey { get; }

    /// <summary>
    /// Parsed unique key constraint information.
    /// </summary>
    /// <value>A collection of parsed unique keys.</value>
    public IEnumerable<UniqueKey> UniqueKeys { get; }

    /// <summary>
    /// Parsed check constraint information.
    /// </summary>
    /// <value>A collection of parsed check constraint.</value>
    public IEnumerable<Check> Checks { get; }

    /// <summary>
    /// Parsed foreign key constraint information.
    /// </summary>
    /// <value>A collection of parsed foreign keys.</value>
    public IEnumerable<ForeignKey> ParentKeys { get; }

    /// <summary>
    /// Whether the table was declared <c>WITHOUT ROWID</c>, and therefore has no implicit rowid
    /// for an <c>INTEGER PRIMARY KEY</c> column to alias.
    /// </summary>
    /// <value><see langword="true" /> if the table has no rowid; otherwise, <see langword="false" />.</value>
    public bool IsWithoutRowId { get; }

    /// <summary>
    /// Creates an empty parsed table definition, using a given <c>CREATE TABLE</c> statement.
    /// </summary>
    /// <param name="definition">A table definition.</param>
    /// <returns>A parsed table definition that has no information.</returns>
    public static ParsedTableData Empty(string definition) => new(definition);
}