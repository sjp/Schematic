using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// Tokenizes SQL Server expressions using the ScriptDom parser.
/// </summary>
internal static class ScriptDomTokenizer
{
    /// <summary>
    /// Tokenizes a SQL Server expression into its constituent tokens.
    /// </summary>
    /// <param name="sql">A SQL expression.</param>
    /// <param name="paramName">The name of the calling argument that supplied <paramref name="sql"/>, used when reporting parse failures.</param>
    /// <returns>The tokens parsed from the expression.</returns>
    /// <exception cref="ArgumentException"><paramref name="sql"/> could not be parsed as a SQL expression.</exception>
    public static IList<TSqlParserToken> Tokenize(string sql, string paramName)
    {
        // ScriptDom parsers are not thread-safe, so a parser is constructed per call rather than
        // shared between invocations. Construction is cheap relative to tokenizing the input.
        var parser = new TSql180Parser(true, SqlEngineType.All);

        using var reader = new StringReader(sql);
        var tokens = parser.GetTokenStream(reader, out var errors);

        var sqlErrors = errors ?? [];
        if (sqlErrors.Count > 0)
        {
            var parserErrorMessages = sqlErrors.Select(e => e.Message ?? string.Empty).Join(", ");
            throw new ArgumentException($"Could not parse the given expression as a SQL expression. Given: {sql}. Error: {parserErrorMessages}", paramName);
        }

        return tokens;
    }
}
