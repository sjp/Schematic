using System;
using SJP.Schematic.Sqlite.Exceptions;
using SJP.Schematic.Sqlite.Parsing.Antlr;

namespace SJP.Schematic.Sqlite.Parsing;

/// <summary>
/// A parser for SQLite <c>CREATE TABLE</c> definitions.
/// </summary>
public class SqliteTableParser
{
    /// <summary>
    /// Parses a <c>CREATE TABLE</c> definition into structured table information.
    /// </summary>
    /// <param name="definition">The textual definition of the <c>CREATE TABLE</c> statement.</param>
    /// <returns>Parsed data for a <c>CREATE TABLE</c> definition.</returns>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is <see langword="null" />, empty or whitespace.</exception>
    /// <exception cref="SqliteTableParsingException">The definition could not be parsed.</exception>
    public ParsedTableData Parse(string definition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definition);

        try
        {
            var parseTree = SqliteDdlParser.ParseTableDefinition(definition);
            return SqliteTableDefinitionBuilder.Build(definition, parseTree);
        }
        catch (SqliteSyntaxErrorException ex)
        {
            throw new SqliteTableParsingException(ex.Message, ex);
        }
    }
}
