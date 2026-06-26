using System;
using SJP.Schematic.Sqlite.Exceptions;
using SJP.Schematic.Sqlite.Parsing.Antlr;

namespace SJP.Schematic.Sqlite.Parsing;

/// <summary>
/// A parser for SQLite <c>CREATE TRIGGER</c> definitions.
/// </summary>
public class SqliteTriggerParser
{
    /// <summary>
    /// Parses a <c>CREATE TRIGGER</c> definition into structured trigger information.
    /// </summary>
    /// <param name="definition">The textual definition of the <c>CREATE TRIGGER</c> statement.</param>
    /// <returns>Parsed data for a <c>CREATE TRIGGER</c> definition.</returns>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is <see langword="null" />, empty or whitespace.</exception>
    /// <exception cref="SqliteTriggerParsingException">The definition could not be parsed.</exception>
    public ParsedTriggerData Parse(string definition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definition);

        try
        {
            var parseTree = SqliteDdlParser.ParseTriggerDefinition(definition);
            return SqliteTriggerDefinitionBuilder.Build(parseTree);
        }
        catch (SqliteSyntaxErrorException ex)
        {
            throw new SqliteTriggerParsingException(ex.Message, ex);
        }
    }
}
